using CommunityToolkit.Maui;
using Money_Spending_Tracker.Features.TransactionData;
using System.Globalization;

namespace Money_Spending_Tracker.Features.Transactions;

[QueryProperty(nameof(FilterDateBeginString), nameof(FilterDateBeginString))]
[QueryProperty(nameof(FilterDateEndString), nameof(FilterDateEndString))]
[QueryProperty(nameof(FilterTagIdString), nameof(FilterTagIdString))]
[QueryProperty(nameof(FilterAccountIdString), nameof(FilterAccountIdString))]
public partial class TransactionListPage : ContentPage
{
    private DateTime? _filterDateBegin;
    private DateTime? _filterDateEnd;
    private int? _filterTagId;
    private Guid? _filterAccountId;

    public string FilterDateBeginString
    {
        get => _filterDateBegin?.ToString("o") ?? string.Empty;
        set => _filterDateBegin = string.IsNullOrEmpty(value) ? null : DateTime.Parse(value, CultureInfo.InvariantCulture);
    }

    public string FilterDateEndString
    {
        get => _filterDateEnd?.ToString("o") ?? string.Empty;
        set => _filterDateEnd = string.IsNullOrEmpty(value) ? null : DateTime.Parse(value, CultureInfo.InvariantCulture);
    }

    public string FilterTagIdString
    {
        get => _filterTagId?.ToString() ?? string.Empty;
        set => _filterTagId = string.IsNullOrEmpty(value) ? null : int.Parse(value);
    }

    public string FilterAccountIdString
    {
        get => _filterAccountId?.ToString() ?? string.Empty;
        set => _filterAccountId = string.IsNullOrEmpty(value) ? null : Guid.Parse(value);
    }

    private readonly TransactionListViewModel _viewModel;

    public TransactionListPage(ITransactionDataService transactionDataService, IPopupService popupService)
    {
        InitializeComponent();
        _viewModel = new TransactionListViewModel(transactionDataService, popupService);
        BindingContext = _viewModel;

        Loaded += TransactionListPage_Loaded;
    }

    private async void TransactionListPage_Loaded(object? sender, EventArgs e)
    {
        await _viewModel.StartAsync(_filterDateBegin, _filterDateEnd, _filterTagId, _filterAccountId);
    }
}