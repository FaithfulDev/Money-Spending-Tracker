using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Onboarding;

[QueryProperty(nameof(ReferenceId), nameof(ReferenceId))]
public partial class OnboardingAccountsPage : ContentPage
{
    public string? ReferenceId { get; set; }

    private OnboardingAccountsViewModel? _viewModel;
    private readonly DatabaseService _databaseService;

    public OnboardingAccountsPage(DatabaseService databaseService)
    {
        InitializeComponent();
        Loaded += OnboardingAccountsPage_Loaded;

        _databaseService = databaseService;
    }

    private async void OnboardingAccountsPage_Loaded(object? sender, EventArgs e)
    {
        _viewModel = new OnboardingAccountsViewModel(_databaseService);
        BindingContext = _viewModel;

        await _viewModel.Start(ReferenceId!);
    }
}