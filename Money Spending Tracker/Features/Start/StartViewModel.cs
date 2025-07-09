using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;
using Money_Spending_Tracker.Features.Onboarding;

namespace Money_Spending_Tracker.Features.Start;

internal partial class StartViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UnlockWithBiometricCommand))]
    private bool _isWorking = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private string? _password;

    private readonly DatabaseService _databaseService;

    public StartViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task Start()
    {
        if (!_databaseService.DoesDatabaseExist())
        {
            await Shell.Current.GoToAsync($"//{nameof(OnboardingPage)}");
            return;
        }

        // Try to unlock the database using fingerprint authentication.
        if (await _databaseService.UnlockAndInitializeAsync())
        {
            await PostUnlock();
            return;
        }

        //Unlock failed. User will need to enter password.
        IsWorking = false;
    }

    [RelayCommand(CanExecute = nameof(IsUnlockExecutable))]
    private async Task Unlock()
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

        if (!isUnlocked)
        {
            IsWorking = false;
            await Shell.Current.DisplayAlert("Error", "Invalid password. Please try again.", "OK");
            return;
        }

        await PostUnlock();
    }

    private bool IsUnlockExecutable()
    {
        return !string.IsNullOrEmpty(Password) && !IsWorking;
    }

    private async Task<bool> IsOnboardingNeeded()
    {
        var context = _databaseService.CreateDbContext();

        // Check if the database is empty, which indicates that onboarding is needed.
        if (!await context.Accounts.AnyAsync())
        {
            return true;
        }

        return false;
    }

    private async Task PostUnlock()
    {
        // Database was unlocked. We need to check if onboarding is needed.
        if (await IsOnboardingNeeded())
        {
            await Shell.Current.GoToAsync($"//{nameof(OnboardingPage)}");
            return;
        }

        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }

    [RelayCommand(CanExecute = nameof(IsUnlockWithBiometricExecutable))]
    private async Task UnlockWithBiometric()
    {
        await Start();
    }

    private bool IsUnlockWithBiometricExecutable()
    {
        return !IsWorking;
    }
}
