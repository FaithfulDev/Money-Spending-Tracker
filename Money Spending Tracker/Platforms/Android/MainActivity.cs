using Android.App;
using Android.Content;
using Android.Content.PM;
using Money_Spending_Tracker.Features.Accounts;
using Money_Spending_Tracker.Features.Storage;
using System.Diagnostics;

namespace Money_Spending_Tracker
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter([Intent.ActionView], Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "de.faithfuldevapps.moneyspendingtracker", DataHost = "authredirect")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            if (intent?.Data != null && intent.Data.Scheme == "de.faithfuldevapps.moneyspendingtracker")
            {
                var authCallbackUrl = intent.Data.ToString();
                Debug.WriteLine($"Received auth callback URL: {authCallbackUrl}");

                if (AppCache.ReAuthenticationInProgress)
                {
                    AppCache.ReAuthenticationInProgress = false;
                    await Shell.Current.GoToAsync($"//{nameof(CallbackDummyPage)}");
                    return;
                }

                // Handle token extraction and navigation logic here
                var reference = intent.Data.GetQueryParameter("ref");

                var onboardingPath = Preferences.Get("OnboardingPath", string.Empty);

                await Shell.Current.GoToAsync($"{onboardingPath}?ReferenceId={reference}");
            }
        }
    }
}
