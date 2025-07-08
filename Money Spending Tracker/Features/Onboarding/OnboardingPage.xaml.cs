using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;
using Money_Spending_Tracker.Features.Storage;

namespace Money_Spending_Tracker.Features.Onboarding;

public partial class OnboardingPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public OnboardingPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;

        AppCache.OnboardingPath = $"//{nameof(OnboardingPage)}";
        AppCache.OnboardingFinalPage = $"{nameof(HomePage)}";
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