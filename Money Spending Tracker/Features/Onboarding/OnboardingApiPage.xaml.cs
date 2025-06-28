namespace Money_Spending_Tracker.Features.Onboarding;

public partial class OnboardingApiPage : ContentPage
{
    private readonly OnboardingApiViewModel _viewModel;

    public OnboardingApiPage()
    {
        InitializeComponent();

        _viewModel = new OnboardingApiViewModel();
        BindingContext = _viewModel;

        var onboardingPath = Preferences.Get("OnboardingPath", string.Empty);
        Preferences.Set("OnboardingPath", $"{onboardingPath}/{nameof(OnboardingApiPage)}");

        Loaded += OnboardingApiPage_Loaded;
    }

    private async void OnboardingApiPage_Loaded(object? sender, EventArgs e)
    {
        await _viewModel.Start();
    }
}