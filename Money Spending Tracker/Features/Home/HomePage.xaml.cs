namespace Money_Spending_Tracker.Features.Home;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private static void Button_Clicked(object sender, EventArgs e)
    {
#if ANDROID
        MainApplication.TriggerWidgetUpdate();
#endif
    }
}
