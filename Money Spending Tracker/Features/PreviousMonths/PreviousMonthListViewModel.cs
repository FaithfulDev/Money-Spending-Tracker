using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Storage;
using Money_Spending_Tracker.Features.TransactionData;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.PreviousMonths;

internal partial class PreviousMonthListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<PreviousMonthGroup> items = [];

    [ObservableProperty]
    private bool _isLoadingMoreItems;

    [ObservableProperty]
    private PreviousMonthListModel? _selectedItem;

    private bool _hasMoreItems = true;

    private int _page = 0;
    private const int PAGE_SIZE = 20;

    private readonly ITransactionDataService _transactionDataService;
    private DateTimeOffset? _lastKnownTransactionUpdate;

    public PreviousMonthListViewModel(ITransactionDataService transactionDataService)
    {
        _transactionDataService = transactionDataService;
    }

    public async Task StartAsync()
    {
        if (!IsUpdateNeeded())
        {
            return;
        }

        Items.Clear();
        _page = 0;
        _hasMoreItems = true;

        await LoadMoreItems();

        // Remember the last known transaction update time to avoid unnecessary refreshes
        _lastKnownTransactionUpdate = AppCache.LastTransactionUpdate;
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
        var newBalances = await _transactionDataService.GetBalancesGroupedByMonth(_page, PAGE_SIZE);

        _hasMoreItems = newBalances.hasMore;

        IEnumerable<PreviousMonthListModel> previousMonthListModels = newBalances.data
            .Select(balance => new PreviousMonthListModel(balance.date, balance.balance));

        var grouped = previousMonthListModels
            .GroupBy(i => i.MonthYear.Year)
            .Select(g => new PreviousMonthGroup(g.Key, g))
            .ToList();

        var groupedItems = new ObservableCollection<PreviousMonthGroup>(grouped);

#if ANDROID
        // This is a workaround for Android to ensure the UI updates don't trigger an IllegalStateException
        // Something about waiting for the next UI pass. I don't know. It works.
        await Task.Delay(1);
#endif

        foreach (var group in groupedItems)
        {
            if (Items.Any(i => i.Year == group.Year))
            {
                // If the group for this year already exists, add the items to it
                var existingGroup = Items.First(i => i.Year == group.Year);
                foreach (var item in group)
                {
                    existingGroup.Add(item);
                }
                continue;
            }

            Items.Add(new PreviousMonthGroup(group.Year, group));
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
            $"{nameof(PreviousMonthDetailPage)}?" +
            $"{nameof(PreviousMonthDetailPage.MonthYear)}={SelectedItem.MonthYear:yyyy-MM-dd}");

        SelectedItem = null;
    }

    private bool IsUpdateNeeded()
    {
        if (_lastKnownTransactionUpdate == null)
        {
            return true;
        }

        return _lastKnownTransactionUpdate < AppCache.LastTransactionUpdate;
    }
}
