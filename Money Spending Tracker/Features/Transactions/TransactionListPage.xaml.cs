using CommunityToolkit.Maui;
using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.Transactions;

[QueryProperty(nameof(FilterDateBegin), nameof(FilterDateBegin))]
[QueryProperty(nameof(FilterDateEnd), nameof(FilterDateEnd))]
[QueryProperty(nameof(FilterTagId), nameof(FilterTagId))]
[QueryProperty(nameof(FilterAccountId), nameof(FilterAccountId))]
public partial class TransactionListPage : ContentPage
{
    public DateTime? FilterDateBegin { get; set; }

    public DateTime? FilterDateEnd { get; set; }

    public int? FilterTagId { get; set; }

    public Guid? FilterAccountId { get; set; }

    private readonly TransactionListViewModel _viewModel;

    public TransactionListPage(ITransactionDataService transactionDataService, IPopupService popupService)
    {
        InitializeComponent();
        _viewModel = new TransactionListViewModel(transactionDataService, popupService);
        BindingContext = _viewModel;

        NavigatedTo += PreviousMonthListPage_NavigatedTo;
    }

    private async void PreviousMonthListPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        await _viewModel.StartAsync(FilterDateBegin, FilterDateEnd, FilterTagId, FilterAccountId);
    }
}