using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

using AndroidNamespace = Android;

namespace Money_Spending_Tracker.Platforms.Android.Widgets;

[BroadcastReceiver(Label = "Monthly Budget Widget", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/monthly_budget_widget_info")]
public class MonthlyBudgetWidgetProvider : AppWidgetProvider
{
    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context == null || appWidgetManager == null || appWidgetIds == null)
        {
            return;
        }

        foreach (var widgetId in appWidgetIds)
        {
            RemoteViews views = new(context.PackageName, Resource.Layout.monthly_budget_widget_layout);

            // Simulate budget data
            Random random = new();
            double min = -1900.0;
            double max = 200.0;
            double randomValue = min + random.NextDouble() * (max - min);
            string budgetAmount = randomValue.ToString("C2", System.Globalization.CultureInfo.CurrentCulture);

            // Choose color based on value
            bool isOverBudget = randomValue < 0;
            var color = isOverBudget
                ? AndroidNamespace.Graphics.Color.Red
                : AndroidNamespace.Graphics.Color.ParseColor("#00AA00");

            views.SetTextViewText(Resource.Id.budget_number, budgetAmount);
            views.SetTextColor(Resource.Id.budget_number, color);

            appWidgetManager.UpdateAppWidget(widgetId, views);
        }
    }
}

