using CommunityToolkit.Maui;
using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Transactions;

[QueryProperty(nameof(TransactionId), nameof(TransactionId))]
[QueryProperty(nameof(AccountId), nameof(AccountId))]
public partial class TransactionDetailPage : ContentPage
{
    public string? TransactionId { get; set; }

    public string? AccountId { get; set; }

    public TransactionDetailPage(DatabaseService databaseService, IPopupService popupService)
    {
        InitializeComponent();

        BindingContext = new TransactionDetailViewModel(databaseService, popupService);

        NavigatedTo += TransactionDetailPage_NavigatedTo;
    }

    private async void TransactionDetailPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        await ((TransactionDetailViewModel)BindingContext).StartAsync(
            transactionId: Guid.Parse(TransactionId!),
            accountId: Guid.Parse(AccountId!)
        );
    }
}