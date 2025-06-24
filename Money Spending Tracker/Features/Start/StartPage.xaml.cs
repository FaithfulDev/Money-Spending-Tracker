using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;
using System.Diagnostics;

namespace Money_Spending_Tracker.Features.Start;

public partial class StartPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public StartPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;

        Loaded += StartPage_Loaded;
    }

    private async void StartPage_Loaded(object? sender, EventArgs e)
    {
        Debug.WriteLine("StartPage Loaded");

        if (await IsOnboardingNeeded())
        {
            //TODO: Navigate to Onboarding page
            //await Shell.Current.GoToAsync("//yourRoute");
        }

        if (await _databaseService.UnlockAndInitializeAsync())
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        //Unlock failed. User will need to enter password.
        ActivityIndicator.IsRunning = false;

        //TODO: make activity indicator nicer.
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (await _databaseService.UnlockAndInitializeAsync(PasswordEntry.Text))
        {
            // Navigate to the HomePage after successful unlock
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }
        else
        {
            // Show an error message if the unlock failed
            await DisplayAlert("Error", "Invalid password. Please try again.", "OK");
        }
    }

    private async Task<bool> IsOnboardingNeeded()
    {
        //TODO: Implement logic to check if onboarding is needed
        return false;

        //Database exists?
    }
}