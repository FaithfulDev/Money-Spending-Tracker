using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace Money_Spending_Tracker;

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
            double min = -600.0;
            double max = -9.0;
            double randomValue = min + (random.NextDouble() * (max - min));
            string budgetAmount = randomValue.ToString("C2", System.Globalization.CultureInfo.CurrentCulture);

            // Choose color based on value
            bool isOverBudget = randomValue < 0;
            var color = isOverBudget
                ? Android.Graphics.Color.Red
                : Android.Graphics.Color.ParseColor("#00AA00");

            views.SetTextViewText(Resource.Id.budget_number, budgetAmount);
            views.SetTextColor(Resource.Id.budget_number, color);

            // Estimate dimensions (fallback when no resize callback is triggered yet)
            int defaultMinWidthDp = 100;
            int defaultMinHeightDp = 40;

            float textSizeSp = CalculateTextSizeSp(defaultMinWidthDp, defaultMinHeightDp);
            views.SetTextViewTextSize(Resource.Id.budget_number, (int)Android.Util.ComplexUnitType.Sp, textSizeSp);

            appWidgetManager.UpdateAppWidget(widgetId, views);
        }
    }

    public override void OnAppWidgetOptionsChanged(Context? context, AppWidgetManager? appWidgetManager, int appWidgetId, Bundle? newOptions)
    {
        base.OnAppWidgetOptionsChanged(context, appWidgetManager, appWidgetId, newOptions);

        if (context == null || appWidgetManager == null || newOptions == null)
        {
            return;
        }

        // Get the current widget dimensions
        int minWidth = newOptions.GetInt(AppWidgetManager.OptionAppwidgetMinWidth);
        int minHeight = newOptions.GetInt(AppWidgetManager.OptionAppwidgetMinHeight);

        // Determine the scaled text size based on width or height
        float textSizeSp = CalculateTextSizeSp(minWidth, minHeight);

        // Update the RemoteViews with new text size
        RemoteViews views = new(context.PackageName, Resource.Layout.monthly_budget_widget_layout);
        views.SetTextViewTextSize(Resource.Id.budget_number, (int)Android.Util.ComplexUnitType.Sp, textSizeSp);

        appWidgetManager.UpdateAppWidget(appWidgetId, views);
    }

    private static float CalculateTextSizeSp(int widthDp, int heightDp)
    {
        // Use the smaller dimension to decide the size
        int smallerSide = Math.Min(widthDp, heightDp);

        return smallerSide switch
        {
            <= 110 => 18f,
            <= 180 => 24f,
            <= 250 => 30f,
            _ => 36f,
        };
    }
}

