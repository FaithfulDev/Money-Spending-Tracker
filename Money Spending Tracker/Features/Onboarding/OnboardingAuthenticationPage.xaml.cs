namespace Money_Spending_Tracker.Features.Onboarding;

[QueryProperty(nameof(ReferenceId), nameof(ReferenceId))]
public partial class OnboardingAuthenticationPage : ContentPage, IQueryAttributable
{
    /// <summary>
    /// If this page is opened with a reference Id, it means that the user is returning from the authentication process.
    /// </summary>
    public string? ReferenceId { get; set; }

    private OnboardingAuthenticationViewModel? _viewModel;

    public OnboardingAuthenticationPage()
    {
        InitializeComponent();

        var onboardingPath = Preferences.Get("OnboardingPath", string.Empty);
        Preferences.Set("OnboardingPath", $"{onboardingPath}/{nameof(OnboardingAuthenticationPage)}");

        Loaded += OnboardingAuthenticationPage_Loaded;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ReferenceId", out var referenceIdObj) && referenceIdObj is string referenceId)
        {
            ReferenceId = referenceId;
            _viewModel?.Start(ReferenceId);
        }
    }

    private async void OnboardingAuthenticationPage_Loaded(object? sender, EventArgs e)
    {
        _viewModel = new OnboardingAuthenticationViewModel();
        BindingContext = _viewModel;

        await _viewModel.Start(ReferenceId);
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel!.FilterInstitutions();
    }

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return;
        }

        _viewModel!.SetSelectedInstitution(e.SelectedItem.ToString()!);
        ((ListView)sender).SelectedItem = null;
    }
}