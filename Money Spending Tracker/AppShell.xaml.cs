using Money_Spending_Tracker.Features.Onboarding;
using System.Diagnostics;

namespace Money_Spending_Tracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        MauiExceptions.UnhandledException += MauiExceptions_UnhandledException;

        // Register detail pages
        Routing.RegisterRoute(nameof(OnboardingDatabasePage), typeof(OnboardingDatabasePage));
        Routing.RegisterRoute(nameof(OnboardingApiPage), typeof(OnboardingApiPage));
        Routing.RegisterRoute(nameof(OnboardingAuthenticationPage), typeof(OnboardingAuthenticationPage));
        Routing.RegisterRoute(nameof(OnboardingAccountsPage), typeof(OnboardingAccountsPage));
    }

    private static void MauiExceptions_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("Exception: " + ((Exception)e.ExceptionObject).Message);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Current.DisplayAlert("Exception", ((Exception)e.ExceptionObject).Message, "OK");
        });
    }
}
