namespace Money_Spending_Tracker.Features.Settings;

internal static class AppSettings
{
    /// <summary>
    /// Monthly budget for spending.
    /// </summary>
    public static double MonthlyBudget
    {
        get { return Preferences.Default.Get<double>(nameof(MonthlyBudget), 0); }
        set { Preferences.Default.Set(nameof(MonthlyBudget), value); }
    }
}
