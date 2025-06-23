using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace Money_Spending_Tracker;

[BroadcastReceiver(Label = "Monthly Budget Widget", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/monthly_budget_widget_info")]
public class MonthlyBudgetWidgetProvider : AppWidgetProvider
{
    //Approximate cell dimensions in dp
    const int cellWidthDp = 70;
    const int cellHeightDp = 50;

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
            int cellWidth = 2;
            int cellHeight = 1;

            // Determine the scaled text size based on width or height
            float textSizeSp = CalculateTextSizeFromCells(cellWidth, cellHeight);

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
        int minWidthDp = newOptions.GetInt(AppWidgetManager.OptionAppwidgetMinWidth);
        int minHeightDp = newOptions.GetInt(AppWidgetManager.OptionAppwidgetMinHeight);

        Log.Debug("WidgetDebug", $"Widget dimensions: minWidthDp={minWidthDp}, minHeightDp={minHeightDp}");

        int cellWidth = Math.Max(minWidthDp / cellWidthDp, 1);
        int cellHeight = Math.Max(minHeightDp / cellHeightDp, 1);

        // Determine the scaled text size based on width or height
        float textSizeSp = CalculateTextSizeFromCells(cellWidth, cellHeight);

        // Update the RemoteViews with new text size
        RemoteViews views = new(context.PackageName, Resource.Layout.monthly_budget_widget_layout);
        views.SetTextViewTextSize(Resource.Id.budget_number, (int)Android.Util.ComplexUnitType.Sp, textSizeSp);

        appWidgetManager.UpdateAppWidget(appWidgetId, views);
    }

    private static float CalculateTextSizeFromCells(int cellWidth, int cellHeight)
    {
        int area = cellWidth * cellHeight;

        Log.Debug("WidgetDebug", $"Calculating text size for area: {area} (width: {cellWidth}, height: {cellHeight})");

        return area switch
        {
            <= 1 => 14f,
            <= 2 => 22f,   // e.g. 1x2 or 2x1
            <= 3 => 34f,
            <= 4 => 34f,   // e.g. 2x2
            <= 6 => 50f,   // e.g. 3x2
            <= 8 => 60f,
            _ => 60f       // larger widgets
        };
    }
}

