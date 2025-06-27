using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Onboarding;

public partial class OnboardingDatabasePage : ContentPage
{
    private readonly OnboardingDatabaseViewModel _onboardingViewModel;

    public OnboardingDatabasePage(DatabaseService databaseService)
    {
        InitializeComponent();

        _onboardingViewModel = new OnboardingDatabaseViewModel(databaseService);
        BindingContext = _onboardingViewModel;

        Loaded += OnboardingDatabasePage_Loaded;
    }

    private void OnboardingDatabasePage_Loaded(object? sender, EventArgs e)
    {
        _onboardingViewModel.Start();
    }
}