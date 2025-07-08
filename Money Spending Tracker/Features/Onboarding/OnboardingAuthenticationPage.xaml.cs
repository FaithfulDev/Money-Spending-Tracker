using Money_Spending_Tracker.Features.ChromeTabs;
using Money_Spending_Tracker.Features.Storage;

namespace Money_Spending_Tracker.Features.Onboarding;

public partial class OnboardingAuthenticationPage : ContentPage, IQueryAttributable
{
    /// <summary>
    /// If this page is opened with a reference Id, it means that the user is returning from the authentication process.
    /// </summary>
    private string? _referenceId { get; set; }

    private OnboardingAuthenticationViewModel? _viewModel;
    private readonly ICustomTabService _customTabService;

    public OnboardingAuthenticationPage(ICustomTabService customTabService)
    {
        InitializeComponent();

        _customTabService = customTabService;

        AppCache.OnboardingPath = $"{AppCache.OnboardingPath}/{nameof(OnboardingAuthenticationPage)}";

        Loaded += OnboardingAuthenticationPage_Loaded;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ReferenceId", out var referenceIdObj) && referenceIdObj is string referenceId)
        {
            _referenceId = referenceId;
            await _viewModel!.Start(_referenceId);
        }
    }

    private async void OnboardingAuthenticationPage_Loaded(object? sender, EventArgs e)
    {
        _viewModel = new OnboardingAuthenticationViewModel(_customTabService);
        BindingContext = _viewModel;

        await _viewModel.Start(_referenceId);
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