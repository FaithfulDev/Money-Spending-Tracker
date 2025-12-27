using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.TransactionData;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.Transactions;

internal partial class TransactionListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TransactionGroup> items = [];

    [ObservableProperty]
    private bool _isLoadingMoreItems;

    [ObservableProperty]
    private TransactionListModel? _selectedItem;

    private bool _hasMoreItems = true;

    private int _page = 0;
    private const int PAGE_SIZE = 20;

    private readonly ITransactionDataService _transactionDataService;
    private readonly IPopupService _popupService;
    private DateTime? _filterDateBegin = null;
    private DateTime? _filterDateEnd = null;
    private int? _filterTagId = null;
    private Guid? _filterAccountId = null;

    private bool _isInitialized = false;

    public TransactionListViewModel(ITransactionDataService transactionDataService, IPopupService popupService)
    {
        _transactionDataService = transactionDataService;
        _popupService = popupService;
    }

    public async Task StartAsync(DateTime? filterDateBegin, DateTime? filterDateEnd, int? filterTagId, Guid? filterAccountId)
    {
        if (_isInitialized)
        {
            return;
        }

        _filterDateBegin = filterDateBegin;
        _filterDateEnd = filterDateEnd;
        _filterTagId = filterTagId;
        _filterAccountId = filterAccountId;

        await ReQuery();

        _isInitialized = true;
    }

    private async Task ReQuery()
    {
        Items.Clear();
        _page = 0;
        _hasMoreItems = true;

        await LoadMoreItems();
    }

    [RelayCommand]
    private async Task LoadMoreItems()
    {
        if (!_hasMoreItems || IsLoadingMoreItems)
        {
            return;
        }

        IsLoadingMoreItems = true;

        _page++;
        var (data, hasMore) = await _transactionDataService.GetTransactionsGroupedMyMonth(
            page: _page,
            pageSize: PAGE_SIZE,
            dateBegin: _filterDateBegin,
            dateEnd: _filterDateEnd,
            tagId: _filterTagId,
            accountId: _filterAccountId);

        _hasMoreItems = hasMore;

        IEnumerable<TransactionGroup> transactionGroups = data
            .Select(tg => new TransactionGroup(
                monthAndyear: tg.date,
                items: tg.transactions.Select(t => new TransactionListModel(
                    internalTransactionId: t.InternalTransactionId,
                    accountId: t.AccountId,
                    valueDate: t.ValueDate,
                    creditorOrDebtorName: t.CreditorName ?? t.DebtorName ?? "(empty)",
                    remittanceInformation: t.RemittanceInformationStructured ?? t.RemittanceInformationUnstructured,
                    transactionAmount: t.TransactionAmount)
                )
            ));

        var groupedItems = new ObservableCollection<TransactionGroup>(transactionGroups);

#if ANDROID
        // This is a workaround for Android to ensure the UI updates don't trigger an IllegalStateException
        // Something about waiting for the next UI pass. I don't know. It works.
        await Task.Delay(1);
#endif

        foreach (var group in groupedItems)
        {
            if (Items.Any(i => i.MonthAndYear == group.MonthAndYear))
            {
                // If the group for this month/year already exists, add the items to it
                var existingGroup = Items.First(i => i.MonthAndYear == group.MonthAndYear);
                foreach (var item in group)
                {
                    existingGroup.Add(item);
                }
                continue;
            }

            Items.Add(group);
        }

        IsLoadingMoreItems = false;
    }

    [RelayCommand]
    private async Task SelectionChanged()
    {
        if (SelectedItem == null)
        {
            return;
        }

        await Shell.Current.GoToAsync(
            $"{nameof(TransactionDetailPage)}?" +
            $"{nameof(TransactionDetailPage.TransactionId)}={SelectedItem.InternalTransactionId}&" +
            $"{nameof(TransactionDetailPage.AccountId)}={SelectedItem.AccountId}");

        SelectedItem = null;
    }

    [RelayCommand]
    private async Task Filter()
    {
        // Dictionary type is foreced by signature of ShowPopupAsync. Objects may be null.
        var shellParameters = new Dictionary<string, object>
        {
            [TransactionsFilterViewModel.QueryParamDateBegin] = _filterDateBegin!,
            [TransactionsFilterViewModel.QueryParamDateEnd] = _filterDateEnd!,
            [TransactionsFilterViewModel.QueryParamTagId] = _filterTagId!,
            [TransactionsFilterViewModel.QueryParamAccountId] = _filterAccountId!,
        };

        IPopupResult<TransactionFilterPopupResults?> result =
            await _popupService.ShowPopupAsync<TransactionsFilterViewModel, TransactionFilterPopupResults?>(
                shell: Shell.Current,
                options: null,
                shellParameters: shellParameters
            );

        if (result == null || result.WasDismissedByTappingOutsideOfPopup)
        {
            return;
        }

        _filterDateBegin = result.Result?.FilterDateBegin;
        _filterDateEnd = result.Result?.FilterDateEnd;
        _filterTagId = result.Result?.FilterTagId;
        _filterAccountId = result.Result?.FilterAccountId;

        await ReQuery();
    }
}
