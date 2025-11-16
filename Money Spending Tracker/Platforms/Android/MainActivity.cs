using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Work;
using Java.Util.Concurrent;
using Money_Spending_Tracker.Features.Accounts;
using Money_Spending_Tracker.Features.Storage;
using Money_Spending_Tracker.Platforms.Android;
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

        protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FixTitleBarOverlapWithStatusBar();
            ScheduleDailyUpdateTransactionsWork();
        }

        private static void ScheduleDailyUpdateTransactionsWork()
        {
            var constraints = new Constraints.Builder()
                            .SetRequiredNetworkType(NetworkType.Connected!)
                            .Build();

            // Calculate delay until next 3am
            var now = DateTime.Now;
            var next3am = now.Date.AddDays(now.Hour >= 3 ? 1 : 0).AddHours(3);
            var delay = (next3am - now).TotalMilliseconds;
            var timeInterval = TimeSpan.FromDays(1);

            var workRequest = PeriodicWorkRequest.Builder
                .From<UpdateTransactionsJobWorker>(timeInterval)
                .SetInitialDelay((long)delay, TimeUnit.Milliseconds)!
                .SetConstraints(constraints)
                .AddTag(nameof(UpdateTransactionsJobWorker))
                .Build();

            WorkManager.GetInstance(Platform.AppContext).EnqueueUniquePeriodicWork(
                nameof(UpdateTransactionsJobWorker),
                ExistingPeriodicWorkPolicy.Update!,
                (PeriodicWorkRequest)workRequest
            );
        }

        /// <summary>
        /// Adjusts the window layout to prevent the title bar from overlapping with the Android status bar. 
        /// </summary>
        /// <remarks>
        /// This fix is needed after updating to .NET MAUI version 10, as it introduced changes that caused the title bar to 
        /// overlap with the android status bar.
        /// </remarks>
        private static void FixTitleBarOverlapWithStatusBar()
        {
            //Fix title bar overlap with Android status bar
            var window = Platform.CurrentActivity?.Window;
            if (window != null)
            {
                AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(window, true);
            }
        }
    }
}
