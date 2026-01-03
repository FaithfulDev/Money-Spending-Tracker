using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.Settings;
using Money_Spending_Tracker.Features.Storage;

namespace Money_Spending_Tracker.Features.Onboarding;

internal partial class OnboardingApiViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isWorking = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckApiCredentialsCommand))]
    private string? _apiSecretId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckApiCredentialsCommand))]
    private string? _apiSecretKey;

    /// <summary>
    /// Indicates whether the API credentials are being updated or not. 
    /// Updates are trigged by settings page.
    /// </summary>
    private bool _isUpdate;

    public async Task Start(bool isUpdate)
    {
        _isUpdate = isUpdate;

        ApiSecretId = await SecureStorage.GetAsync(StorageKeys.API_SECRET_ID);
        ApiSecretKey = await SecureStorage.GetAsync(StorageKeys.API_SECRET_KEY);
    }

    [RelayCommand(CanExecute = nameof(IsCheckApiCredentialsExecutable))]
    private async Task CheckApiCredentials()
    {
        if (string.IsNullOrEmpty(ApiSecretId) || string.IsNullOrEmpty(ApiSecretKey))
        {
            await Shell.Current.DisplayAlertAsync("Error", "Please check inputs.", "OK");
            return;
        }

        IsWorking = true;

        // Temporarily store the api credentials, in case we need to restore them.
        string? originalApiSecretId = await SecureStorage.GetAsync(StorageKeys.API_SECRET_ID);
        string? originalApiSecretKey = await SecureStorage.GetAsync(StorageKeys.API_SECRET_KEY);

        try
        {
            // Values are used in the api call.
            await SecureStorage.SetAsync(StorageKeys.API_SECRET_ID, ApiSecretId);
            await SecureStorage.SetAsync(StorageKeys.API_SECRET_KEY, ApiSecretKey);

            // Clear the API token to ensure we get a fresh one.
            await SecureStorage.SetAsync(StorageKeys.API_TOKEN, string.Empty);

            var apiClient = new Client(new());
            await apiClient.Retrieve_all_supported_Institutions_in_a_given_countryAsync();
        }
        catch (Exception ex)
        {
            IsWorking = false;
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");

            //Restore original api credentials
            await SecureStorage.SetAsync(StorageKeys.API_SECRET_ID, originalApiSecretId ?? string.Empty);
            await SecureStorage.SetAsync(StorageKeys.API_SECRET_KEY, originalApiSecretKey ?? string.Empty);

            return;
        }

        IsWorking = false;

        if (_isUpdate)
        {
            await Shell.Current.GoToAsync($"//{nameof(SettingsPage)}");
            await Toast.Make("API credentials updated successfully.", ToastDuration.Long).Show();
            return;
        }

        // Navigate to next onboarding step
        await Shell.Current.GoToAsync($"{nameof(OnboardingAuthenticationPage)}");
    }

    private bool IsCheckApiCredentialsExecutable()
    {
        return !string.IsNullOrEmpty(ApiSecretId) &&
               !string.IsNullOrEmpty(ApiSecretKey) &&
               !IsWorking;
    }
}
