using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.Home;

[QueryProperty(nameof(AccountsAdded), nameof(AccountsAdded))]
public partial class HomePage : ContentPage
{
    public bool AccountsAdded { get; set; } = false;

    private readonly ITransactionDataService _transactionDataService;

    public HomePage(ITransactionDataService transactionDataService)
    {
        InitializeComponent();

        _transactionDataService = transactionDataService;

        NavigatedTo += HomePage_NavigatedTo;
    }

    private async void HomePage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        var viewModel = new HomeViewModel(_transactionDataService);
        BindingContext = viewModel;

        await viewModel.StartAsync(AccountsAdded);
    }
}
