using System.Diagnostics;

namespace Money_Spending_Tracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        MauiExceptions.UnhandledException += MauiExceptions_UnhandledException;
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
