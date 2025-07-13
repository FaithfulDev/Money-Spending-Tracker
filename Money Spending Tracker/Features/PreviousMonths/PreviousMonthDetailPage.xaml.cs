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

        Loaded += PreviousMonthDetailPage_Loaded;
    }

    private async void PreviousMonthDetailPage_Loaded(object? sender, EventArgs e)
    {
        var viewModel = (PreviousMonthDetailViewModel)BindingContext;
        await viewModel.StartAsync(new DateOnly(MonthYear.Year, MonthYear.Month, 1));
    }
}