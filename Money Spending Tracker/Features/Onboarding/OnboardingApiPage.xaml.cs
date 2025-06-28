namespace Money_Spending_Tracker.Features.Onboarding;

[QueryProperty(nameof(IsUpdate), nameof(IsUpdate))]
public partial class OnboardingApiPage : ContentPage
{
    public bool IsUpdate { get; set; }

    public OnboardingApiPage()
    {
        InitializeComponent();

        var onboardingPath = Preferences.Get("OnboardingPath", string.Empty);
        Preferences.Set("OnboardingPath", $"{onboardingPath}/{nameof(OnboardingApiPage)}");

        Loaded += OnboardingApiPage_Loaded;
    }

    private async void OnboardingApiPage_Loaded(object? sender, EventArgs e)
    {
        var viewModel = new OnboardingApiViewModel();
        BindingContext = viewModel;

        await viewModel.Start(IsUpdate);
    }
}