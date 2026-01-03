using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using Money_Spending_Tracker.Features.Storage;
using AndroidNamespace = Android;

namespace Money_Spending_Tracker.Platforms.Android.Widgets;

[BroadcastReceiver(Label = "Tag Balance Widget", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/tag_balance_widget_info")]
public class TagBalanceWidgetProvider : AppWidgetProvider
{
    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context == null || appWidgetManager == null || appWidgetIds == null)
        {
            return;
        }

        foreach (var widgetId in appWidgetIds)
        {
            RemoteViews views = new(context.PackageName, Resource.Layout.tag_balance_widget_layout);

            var (tagId, tagName) = AppCache.GetWidget(widgetId);

            views.SetTextViewText(Resource.Id.widget_caption, tagName);

            double tagBalance = AppCache.GetTagValue(tagId);
            string balanceAmountFormatted = tagBalance.ToString(
                "C2", System.Globalization.CultureInfo.CurrentCulture);

            // Choose color based on value
            bool isNegative = tagBalance < 0;
            var color = isNegative
                ? AndroidNamespace.Graphics.Color.Rgb(255, 0, 0) //red
                : AndroidNamespace.Graphics.Color.Rgb(25, 148, 25); //green

            views.SetTextViewText(Resource.Id.balance_number, balanceAmountFormatted);
            views.SetTextColor(Resource.Id.balance_number, color);

            appWidgetManager.UpdateAppWidget(widgetId, views);
        }
    }
}

