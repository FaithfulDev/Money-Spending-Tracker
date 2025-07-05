using AndroidX.Browser.CustomTabs;
using Money_Spending_Tracker.Features.ChromeTabs;
using Uri = Android.Net.Uri;

namespace Money_Spending_Tracker.Platforms.Android;

public class CustomTabService : ICustomTabService
{
    public void OpenUrl(System.Uri uri)
    {
        var activity = Platform.CurrentActivity;
        var builder = new CustomTabsIntent.Builder();
        var customTabsIntent = builder.Build();
        customTabsIntent.LaunchUrl(activity!, Uri.Parse(uri.ToString())!);
    }
}
