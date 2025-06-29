namespace Money_Spending_Tracker.Features.Storage;

internal static class AppCache
{
    /// <summary>
    /// Last time transactions were updated from the API.
    /// </summary>
    public static DateTimeOffset LastTransactionUpdate
    {
        get { return Preferences.Default.Get<DateTimeOffset>(nameof(LastTransactionUpdate), new(2025, 1, 1, 0, 0, 0, new(0, 0, 0))); }
        set { Preferences.Default.Set(nameof(LastTransactionUpdate), value); }
    }

    public static double RemainingMonthlyBudget
    {
        get { return Preferences.Default.Get<double>(nameof(RemainingMonthlyBudget), 0); }
        set { Preferences.Default.Set(nameof(RemainingMonthlyBudget), value); }
    }

    public static double CurrentBalance
    {
        get { return Preferences.Default.Get<double>(nameof(CurrentBalance), 0); }
        set { Preferences.Default.Set(nameof(CurrentBalance), value); }
    }
}
