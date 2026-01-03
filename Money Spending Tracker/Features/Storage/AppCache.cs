using System.Globalization;

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
        set { Preferences.Default.Set(nameof(LastTransactionUpdate), value?.ToString("O") ?? string.Empty); }
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

    public static void SetWidget(int widgetId, int tagId, string tagName)
    {
        Preferences.Default.Set($"Widget_{widgetId}_TagId", tagId);
        Preferences.Default.Set($"Widget_{widgetId}_TagName", tagName);
    }

    public static (Guid LockGuid, DateTime LockDateTime)? UpdateLock
    {
        get
        {
            var lockJsonString = Preferences.Default.Get(nameof(UpdateLock), string.Empty);

            if (string.IsNullOrEmpty(lockJsonString))
            {
                return null;
            }

            var lockJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(lockJsonString);

            return (
                Guid.Parse(lockJson!["Guid"]),
                DateTime.Parse(lockJson!["DateTime"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            );
        }
        set
        {
            var lockJson = value.HasValue
                ? System.Text.Json.JsonSerializer.Serialize(new
                {
                    Guid = value.Value.LockGuid.ToString(),
                    DateTime = value.Value.LockDateTime.ToString("O")
                })
                : string.Empty;

            Preferences.Default.Set(nameof(UpdateLock), lockJson);
        }
    }

    public static (int TagId, string TagName) GetWidget(int widgetId)
    {
        int tagId = Preferences.Default.Get($"Widget_{widgetId}_TagId", -1);
        string tagName = Preferences.Default.Get($"Widget_{widgetId}_TagName", string.Empty);
        return (tagId, tagName);
    }

    public static void SetTagValue(int tagId, double value)
    {
        Preferences.Default.Set($"Tag_{tagId}_Value", value);
    }

    public static double GetTagValue(int tagId)
    {
        return Preferences.Default.Get<double>($"Tag_{tagId}_Value", 0);
    }

    public static void ClearTagValue(int tagId)
    {
        Preferences.Default.Remove($"Tag_{tagId}_Value");
    }
}
