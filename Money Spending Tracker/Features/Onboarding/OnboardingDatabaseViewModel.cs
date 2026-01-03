using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Onboarding;

internal partial class OnboardingDatabaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isWorking = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    private string? _password;

    private readonly DatabaseService _databaseService;

    public OnboardingDatabaseViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public void Start()
    {
        // When user are being redirected to this page, then the database should be deleted.
        _databaseService.DeleteDatabase();
    }

    [RelayCommand(CanExecute = nameof(IsCreateDatabaseExecutable))]
    private async Task CreateDatabase()
    {
        if (string.IsNullOrEmpty(Password))
        {
            await Shell.Current.DisplayAlertAsync("Error", "Password cannot be empty.", "OK");
            return;
        }

        IsWorking = true;

        _databaseService.DeleteDatabase();

        try
        {
            await _databaseService.UnlockAndInitializeAsync(Password);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");

            // In case the database was created, but some other error occurred.
            _databaseService.DeleteDatabase();

            IsWorking = false;
            return;
        }

        IsWorking = false;

        // Navigate to next onboarding step
        await Shell.Current.GoToAsync($"{nameof(OnboardingApiPage)}");
    }

    private bool IsCreateDatabaseExecutable()
    {
        return !string.IsNullOrEmpty(Password) && !IsWorking;
    }
}
