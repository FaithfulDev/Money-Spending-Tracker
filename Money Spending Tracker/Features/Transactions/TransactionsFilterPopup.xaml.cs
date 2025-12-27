namespace Money_Spending_Tracker.Features.Transactions;

public partial class TransactionsFilterPopup : ContentView
{
    public TransactionsFilterPopup(TransactionsFilterViewModel filterPopupViewModel)
    {
        InitializeComponent();
        BindingContext = filterPopupViewModel;

        Loaded += TransactionsFilterPopup_Loaded;
    }

    private async void TransactionsFilterPopup_Loaded(object? sender, EventArgs e)
    {
        await ((TransactionsFilterViewModel)BindingContext).StartAsync();
    }
}