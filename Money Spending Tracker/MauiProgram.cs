using Microsoft.Extensions.Logging;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;
using Money_Spending_Tracker.Features.Start;
using Plugin.Fingerprint;

namespace Money_Spending_Tracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
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

            builder.Services.AddSingleton<DatabaseService>();

            builder.Services.AddTransient<StartPage>();
            builder.Services.AddTransient<HomePage>();

            return builder.Build();
        }
    }
}
