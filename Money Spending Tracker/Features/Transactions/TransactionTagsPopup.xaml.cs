namespace Money_Spending_Tracker.Features.Transactions;

public partial class TransactionTagsPopup : ContentView
{
    public TransactionTagsPopup(TransactionTagsViewModel transactionTagsViewModel)
    {
        InitializeComponent();
        BindingContext = transactionTagsViewModel;

        Loaded += TransactionTagsPopup_Loaded;
    }

    private async void TransactionTagsPopup_Loaded(object? sender, EventArgs e)
    {
        await ((TransactionTagsViewModel)BindingContext).StartAsync();
    }
}