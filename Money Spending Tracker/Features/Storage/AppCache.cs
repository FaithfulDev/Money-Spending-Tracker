namespace Money_Spending_Tracker.Features.Storage;

internal static class AppCache
{
    /// <summary>
    /// Last time transactions were updated from the API.
    /// </summary>
    public static DateTime? LastTransactionUpdate
    {
        get
        {
            var lastUpdate = Preferences.Default.Get(nameof(LastTransactionUpdate), string.Empty);

            if (DateTime.TryParse(lastUpdate, out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null;
        }
        set { Preferences.Default.Set(nameof(LastTransactionUpdate), value?.ToString("yyyy-MM-dd") ?? string.Empty); }
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

    public static string OnboardingPath
    {
        get { return Preferences.Default.Get(nameof(OnboardingPath), string.Empty); }
        set { Preferences.Default.Set(nameof(OnboardingPath), value); }
    }

    public static string OnboardingFinalPage
    {
        get { return Preferences.Default.Get(nameof(OnboardingFinalPage), string.Empty); }
        set { Preferences.Default.Set(nameof(OnboardingFinalPage), value); }
    }

    public static bool ReAuthenticationInProgress
    {
        get { return Preferences.Default.Get(nameof(ReAuthenticationInProgress), false); }
        set { Preferences.Default.Set(nameof(ReAuthenticationInProgress), value); }
    }
}
