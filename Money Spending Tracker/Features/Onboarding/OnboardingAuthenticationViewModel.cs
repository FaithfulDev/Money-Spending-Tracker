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
    private string? _institutionId;

    [ObservableProperty]
    private string? _institutionName;

    [ObservableProperty]
    private List<string> _filteredInstitutions = [];

    private ICollection<Integration> _allInstitutions = [];
    private readonly Client _apiClient = new(new());

    public async Task Start(string? referenceId)
    {
        IsWorking = true;

        // If we have a reference ID, we will try to check the requisition.
        if (!string.IsNullOrEmpty(referenceId) && await Callback(referenceId))
        {
            await Shell.Current.GoToAsync($"{nameof(OnboardingAccountsPage)}?ReferenceId={referenceId}");
            IsWorking = false;
            return;
        }

        // Retrieve all institutions. We filter them later.
        _allInstitutions = await _apiClient.Retrieve_all_supported_Institutions_in_a_given_countryAsync();

        IsWorking = false;
    }

    public async Task<bool> Callback(string referenceId)
    {
        Requisition? requisition = null;

        try
        {
            requisition = await _apiClient.Requisition_by_idAsync(Guid.Parse(referenceId!));
        }
        catch (ApiException ex) when (ex.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
        {
            // Ignore this exception, as it means that the requisition was not found.
            // This should not happen, but just in case, we handle it gracefully.
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"An error occurred while retrieving the requisition: {ex.Message}. " +
                $"Please repeat process.", "OK");
            return false;
        }

        if (requisition == null)
        {
            await Shell.Current.DisplayAlert("Error", "Requisition not found. Please repeat process.", "OK");
            return false;
        }

        if (requisition.Status != StatusEnum.LN)
        {
            await Shell.Current.DisplayAlert("Error", "Requisition authentication was unsuccessful. Please repeat process.", "OK");
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(IsAuthenticateExecutable))]
    private async Task Authenticate()
    {
        if (string.IsNullOrEmpty(InstitutionId))
        {
            await Shell.Current.DisplayAlert("Error", "Please check inputs.", "OK");
            return;
        }

        IsWorking = true;

        var agreement = await _apiClient.Create_EUAAsync(new()
        {
            Institution_id = InstitutionId,
            Access_scope = ["details", "transactions"],
        });

        var requisition = await _apiClient.Create_requisitionAsync(new()
        {
            Institution_id = InstitutionId,
            Redirect = new Uri("de.faithfuldevapps.moneyspendingtracker://authredirect"),
            Agreement = agreement.Id,
        });

        var authUrl = requisition.Link;
        await Launcher.Default.OpenAsync(authUrl);
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
        return !string.IsNullOrEmpty(InstitutionId) && !IsWorking;
    }
}
