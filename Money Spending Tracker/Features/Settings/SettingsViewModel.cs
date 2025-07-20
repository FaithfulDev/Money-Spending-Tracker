using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Features.BackgroundJob;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Onboarding;
using Money_Spending_Tracker.Features.Start;
using Money_Spending_Tracker.Features.Storage;

namespace Money_Spending_Tracker.Features.Settings;

internal partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _monthlyBudget;

    [ObservableProperty]
    private bool _isValid = true;

    [ObservableProperty]
    private bool _isWorking = false;

    private readonly DatabaseService _databaseService;

    public SettingsViewModel(DatabaseService databaseService)
    {
        MonthlyBudget = AppSettings.MonthlyBudget.ToString();
        _databaseService = databaseService;
    }

    [RelayCommand]
    private async Task Save()
    {
        Validate();

        if (!IsValid)
        {
            await Toast.Make("Failed to save settings!", ToastDuration.Long).Show();
            return;
        }

        AppSettings.MonthlyBudget = double.Parse(MonthlyBudget!, System.Globalization.CultureInfo.InvariantCulture);

        await Toast.Make("Settings saved successfully.", ToastDuration.Short).Show();
    }

    private void Validate()
    {
        IsValid = true;

        if (string.IsNullOrWhiteSpace(MonthlyBudget))
        {
            IsValid = false;
        }

        if (!double.TryParse(MonthlyBudget, System.Globalization.CultureInfo.InvariantCulture, out _))
        {
            IsValid = false;
        }
    }

    [RelayCommand]
    private async Task UpdateApiCredentials()
    {
        await Shell.Current.GoToAsync($"{nameof(OnboardingApiPage)}?IsUpdate=true");
    }

    [RelayCommand]
    private async Task DeleteTransactions()
    {
        if (await Shell.Current.DisplayAlert(
            "Delete Transactions",
            "Are you sure you want to delete all transactions? This action cannot be undone.",
            "Yes", "No"))
        {
            IsWorking = true;
            try
            {
                var dbContext = _databaseService.CreateDbContext();

                await dbContext.Transactions.ExecuteDeleteAsync();
                await dbContext.SaveChangesAsync();

                AppCache.LastTransactionUpdate = null;
                AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget;
                AppCache.CurrentBalance = 0;

#if ANDROID
                MainApplication.TriggerWidgetUpdate();
#endif

                await Toast.Make("All transactions deleted successfully.", ToastDuration.Short).Show();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete transactions: {ex.Message}", "OK");
            }
            finally
            {
                IsWorking = false;
            }
        }
    }

    [RelayCommand]
    private async Task DeleteAllData()
    {
        if (await Shell.Current.DisplayAlert(
            "Delete All Data",
            "Are you sure you want to delete all data? This action cannot be undone.",
            "Yes", "No"))
        {
            IsWorking = true;
            try
            {
                _databaseService.DeleteDatabase();
                await SecureStorage.SetAsync(StorageKeys.API_SECRET_ID, string.Empty);
                await SecureStorage.SetAsync(StorageKeys.API_SECRET_KEY, string.Empty);
                await SecureStorage.SetAsync(StorageKeys.API_TOKEN, string.Empty);
                await SecureStorage.SetAsync(StorageKeys.DB_PASSWORD, string.Empty);

                //Clear the cache and preferences
                Preferences.Default.Clear();

                await Toast.Make("All data deleted successfully.", ToastDuration.Short).Show();
                await Shell.Current.GoToAsync($"//{nameof(StartPage)}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete all data: {ex.Message}", "OK");
            }
            finally
            {
                IsWorking = false;
            }
        }
    }

    [RelayCommand]
    private async Task CheckJobs()
    {
        await Shell.Current.GoToAsync($"{nameof(BackgroundJobsPage)}");
    }
}
