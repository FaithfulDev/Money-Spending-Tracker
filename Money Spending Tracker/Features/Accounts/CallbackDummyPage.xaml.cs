namespace Money_Spending_Tracker.Features.Accounts;

public partial class CallbackDummyPage : ContentPage
{
    public CallbackDummyPage()
    {
        InitializeComponent();
        NavigatedTo += CallbackDummyPage_NavigatedTo;
    }

    private static async void CallbackDummyPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(AccountsPage)}");
    }
}