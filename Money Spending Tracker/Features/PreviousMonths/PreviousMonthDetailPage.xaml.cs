using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.PreviousMonths;

[QueryProperty(nameof(MonthYear), nameof(MonthYear))]
public partial class PreviousMonthDetailPage : ContentPage
{
    public DateTime MonthYear { get; set; }

    public PreviousMonthDetailPage(ITransactionDataService transactionDataService)
    {
        InitializeComponent();

        var viewModel = new PreviousMonthDetailViewModel(transactionDataService);
        BindingContext = viewModel;

        NavigatedTo += PreviousMonthDetailPage_NavigatedTo;
    }

    private async void PreviousMonthDetailPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        var viewModel = (PreviousMonthDetailViewModel)BindingContext;
        await viewModel.StartAsync(new DateOnly(MonthYear.Year, MonthYear.Month, 1));
    }
}