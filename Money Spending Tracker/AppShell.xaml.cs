using Money_Spending_Tracker.Features.BackgroundJob;
using Money_Spending_Tracker.Features.Onboarding;
using Money_Spending_Tracker.Features.PreviousMonths;
using Money_Spending_Tracker.Features.Tags;
using Money_Spending_Tracker.Features.Transactions;
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
        Routing.RegisterRoute(nameof(PreviousMonthDetailPage), typeof(PreviousMonthDetailPage));
        Routing.RegisterRoute(nameof(BackgroundJobsPage), typeof(BackgroundJobsPage));
        Routing.RegisterRoute(nameof(EditTagNameModal), typeof(EditTagNameModal));
        Routing.RegisterRoute(nameof(TransactionDetailPage), typeof(TransactionDetailPage));
        Routing.RegisterRoute(nameof(TransactionListPage), typeof(TransactionListPage));
    }

    private static async void MauiExceptions_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("Exception: " + ((Exception)e.ExceptionObject).Message);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Current.DisplayAlertAsync("Exception", ((Exception)e.ExceptionObject).Message, "OK");
        });
    }
}
