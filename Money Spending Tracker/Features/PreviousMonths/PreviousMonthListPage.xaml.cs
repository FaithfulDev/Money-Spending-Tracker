using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.PreviousMonths;

public partial class PreviousMonthListPage : ContentPage
{
    private readonly PreviousMonthListViewModel _viewModel;

    public PreviousMonthListPage(ITransactionDataService transactionDataService)
    {
        InitializeComponent();
        _viewModel = new PreviousMonthListViewModel(transactionDataService);
        BindingContext = _viewModel;

        NavigatedTo += PreviousMonthListPage_NavigatedTo;
    }

    private async void PreviousMonthListPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        await _viewModel.StartAsync();
    }
}