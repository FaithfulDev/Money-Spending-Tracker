using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;

namespace Money_Spending_Tracker.Features.Start;

internal partial class StartViewModel : ObservableObject
{
    [ObservableProperty]
    public bool _isWorking = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public string? _password;

    private readonly DatabaseService _databaseService;

    public StartViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task Start()
    {
        //TODO: Check if onboarding is needed
        //if (await IsOnboardingNeeded())
        //{
        //    //TODO: Navigate to Onboarding page
        //    //await Shell.Current.GoToAsync("//yourRoute");
        //}

        if (await _databaseService.UnlockAndInitializeAsync())
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        //Unlock failed. User will need to enter password.
        IsWorking = false;
    }

    [RelayCommand(CanExecute = nameof(IsLoginExecutable))]
    private async Task Login()
    {
        if (string.IsNullOrEmpty(Password))
        {
            await Shell.Current.DisplayAlert("Error", "Password cannot be empty.", "OK");
            return;
        }

        IsWorking = true;

        bool isUnlocked = false;

        try
        {
            isUnlocked = await _databaseService.UnlockAndInitializeAsync(Password);
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            IsWorking = false;
            return;
        }

        if (isUnlocked)
        {
            // Navigate to the HomePage after successful unlock
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            return;
        }
        else
        {
            // Show an error message if the unlock failed
            await Shell.Current.DisplayAlert("Error", "Invalid password. Please try again.", "OK");
        }

        IsWorking = false;
    }

    private bool IsLoginExecutable()
    {
        return !string.IsNullOrEmpty(Password) && !IsWorking;
    }
}
