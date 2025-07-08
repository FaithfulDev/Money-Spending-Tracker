using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;
using Money_Spending_Tracker.Features.Onboarding;
using Money_Spending_Tracker.Features.Settings;
using Money_Spending_Tracker.Features.Start;
using Plugin.Fingerprint;
using Money_Spending_Tracker.Features.Accounts;
using Money_Spending_Tracker.Features.TransactionData;



#if ANDROID
using Money_Spending_Tracker.Features.ChromeTabs;
using Money_Spending_Tracker.Platforms.Android;
#endif

namespace Money_Spending_Tracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontAwesomeSolid");
                    fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontAwesomeRegular");
                });

            SQLitePCL.Batteries_V2.Init();
            CrossFingerprint.SetCurrentActivityResolver(() => Platform.CurrentActivity);

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddTransient<ICustomTabService, CustomTabService>();
#endif

            builder.Services.AddSingleton<DatabaseService>();

            builder.Services.AddTransient<StartPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AccountsPage>();
            builder.Services.AddTransient<ITransactionDataService, TransactionDataService>();

            return builder.Build();
        }
    }
}
