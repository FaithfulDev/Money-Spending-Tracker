using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using Money_Spending_Tracker.Features.Storage;
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

            double remainingBudget = AppCache.RemainingMonthlyBudget;

            string budgetAmountFormatted = remainingBudget.ToString(
                "C2", System.Globalization.CultureInfo.CurrentCulture);

            // Choose color based on value
            bool isOverBudget = remainingBudget < 0;
            var color = isOverBudget
                ? AndroidNamespace.Graphics.Color.Red
                : AndroidNamespace.Graphics.Color.Black;

            views.SetTextViewText(Resource.Id.budget_number, budgetAmountFormatted);
            views.SetTextColor(Resource.Id.budget_number, color);

            appWidgetManager.UpdateAppWidget(widgetId, views);
        }
    }
}

