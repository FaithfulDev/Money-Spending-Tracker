using Money_Spending_Tracker.Features.ChromeTabs;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.Accounts;

[QueryProperty(nameof(AccountsAdded), nameof(AccountsAdded))]
public partial class AccountsPage : ContentPage
{
    public bool AccountsAdded { get; set; } = false;

    public AccountsPage(DatabaseService databaseService, ITransactionDataService transactionDataService,
        ICustomTabService customTabService)
    {
        InitializeComponent();
        BindingContext = new AccountsViewModel(databaseService, transactionDataService, customTabService);

        NavigatedTo += AccountsPage_NavigatedTo;
    }

    private async void AccountsPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        var viewModel = (AccountsViewModel)BindingContext;
        await viewModel.StartAsync(AccountsAdded);
    }
}