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
using Money_Spending_Tracker.Features.BackgroundJob;
using Money_Spending_Tracker.Features.Transactions;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Toolkit.Hosting;

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
                .ConfigureSyncfusionToolkit()
                .UseMauiCommunityToolkit(static options =>
                {
                    options.SetPopupDefaults(new DefaultPopupSettings
                    {
                        CanBeDismissedByTappingOutsideOfPopup = true,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = new(20, 20),
                        Padding = 0,

                    });
                    options.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings
                    {
                        CanBeDismissedByTappingOutsideOfPopup = true,
                        Shadow = new Shadow
                        {
                            Opacity = 0.3f,
                            Radius = 3,
                            Offset = new Point(5, 5)
                        },
                        Shape = new Rectangle()
                    });
                })
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
            builder.Services.AddSingleton<IBackgroundService, BackgroundService>();
#endif

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ITransactionDataService, TransactionDataService>();

            builder.Services.AddTransient<StartPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AccountsPage>();
            builder.Services.AddTransient<TransactionListPage>();

            builder.Services.AddTransientPopup<TransactionsFilterPopup, TransactionsFilterViewModel>();
            builder.Services.AddTransientPopup<TransactionTagsPopup, TransactionTagsViewModel>();

            MauiServiceProvider.Current = builder.Services.BuildServiceProvider();

            return builder.Build();
        }
    }
}
