using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Api;

namespace Money_Spending_Tracker.Features.Onboarding;

internal partial class OnboardingAuthenticationViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isWorking = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuthenticateCommand))]
    private string? _InstitutionId;

    [ObservableProperty]
    private string? _InstitutionName;

    [ObservableProperty]
    private List<string> _filteredInstitutions = [];

    private ICollection<Integration> _allInstitutions = [];

    public async Task Start()
    {
        Client apiClient = new(new());

        // Retrieve all institutions. We filter them later.
        _allInstitutions = await apiClient.Retrieve_all_supported_Institutions_in_a_given_countryAsync();

        //FilteredInstitutions = _allInstitutions
        //    .Select(i => i.Name)
        //    .OrderBy(name => name)
        //    .Take(30) // Limit to 30 institutions for performance
        //    .ToList();
    }

    [RelayCommand(CanExecute = nameof(IsAuthenticateExecutable))]
    private async Task Authenticate()
    {
        //TODO
        if (string.IsNullOrEmpty(InstitutionId))
        {
            await Shell.Current.DisplayAlert("Error", "Please check inputs.", "OK");
            return;
        }

        IsWorking = true;

        //TODO

        IsWorking = false;

        //TODO
        // Navigate to next onboarding step
        //await Shell.Current.GoToAsync($"//{nameof(OnboardingPage)}");
        await Shell.Current.DisplayAlert("okay", "okay", "OK");
    }

    public void FilterInstitutions()
    {
        if (string.IsNullOrEmpty(InstitutionName))
        {
            FilteredInstitutions = [];
            return;
        }

        FilteredInstitutions = _allInstitutions
            .Where(i => string.IsNullOrEmpty(InstitutionName) || i.Name.Contains(InstitutionName, StringComparison.OrdinalIgnoreCase))
            .Select(i => i.Name)
            .OrderBy(i => i)
            .Take(30) // Limit to 30 institutions for performance
            .ToList();
    }

    public void SetSelectedInstitution(string institutionName)
    {
        InstitutionId = _allInstitutions.FirstOrDefault(i => i.Name.Equals(institutionName, StringComparison.OrdinalIgnoreCase))?.Id;
        InstitutionName = institutionName;
        FilteredInstitutions = [];
    }

    private bool IsAuthenticateExecutable()
    {
        return !string.IsNullOrEmpty(InstitutionId);
    }
}
