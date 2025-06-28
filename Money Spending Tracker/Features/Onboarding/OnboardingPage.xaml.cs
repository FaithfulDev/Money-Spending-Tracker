using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Onboarding;

public partial class OnboardingPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public OnboardingPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;

        Preferences.Set("OnboardingPath", $"//{nameof(OnboardingPage)}");
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (_databaseService.DoesDatabaseExist())
        {
            await Shell.Current.GoToAsync($"{nameof(OnboardingApiPage)}");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(OnboardingDatabasePage)}");
    }
}