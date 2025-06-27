namespace Money_Spending_Tracker.Features.Onboarding;

public partial class OnboardingAuthenticationPage : ContentPage
{
    private readonly OnboardingAuthenticationViewModel _viewModel;

    public OnboardingAuthenticationPage()
    {
        InitializeComponent();

        _viewModel = new OnboardingAuthenticationViewModel();
        BindingContext = _viewModel;

        Loaded += OnboardingAuthenticationPage_Loaded;
    }

    private async void OnboardingAuthenticationPage_Loaded(object? sender, EventArgs e)
    {
        await _viewModel.Start();
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.FilterInstitutions();
    }

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return;
        }

        _viewModel.SetSelectedInstitution(e.SelectedItem.ToString()!);
        ((ListView)sender).SelectedItem = null;
    }
}