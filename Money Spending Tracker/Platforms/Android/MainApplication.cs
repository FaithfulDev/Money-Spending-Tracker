using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Runtime;
using Money_Spending_Tracker.Platforms.Android.Widgets;

namespace Money_Spending_Tracker
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public static void TriggerWidgetUpdate()
        {
            var context = Context;
            var widgetManager = AppWidgetManager.GetInstance(context);

            List<(ComponentName component, Type providerType)> widgetComponents = [
                (new ComponentName(context, Java.Lang.Class.FromType(typeof(MonthlyBudgetWidgetProvider))), typeof(MonthlyBudgetWidgetProvider)),
                (new ComponentName(context, Java.Lang.Class.FromType(typeof(TagBalanceWidgetProvider))), typeof(TagBalanceWidgetProvider)),
            ];

            TriggerWidgetUpdate_Internal(context, widgetManager!, widgetComponents);
        }

        private static void TriggerWidgetUpdate_Internal(Context context, AppWidgetManager appWidgetManager,
             List<(ComponentName component, Type providerType)> widgetComponents)
        {
            foreach (var (component, providerType) in widgetComponents)
            {
                int[] widgetIds = appWidgetManager.GetAppWidgetIds(component)!;

                Intent updateIntent = new(context, providerType);
                updateIntent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
                updateIntent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, widgetIds);

                context.SendBroadcast(updateIntent);
            }
        }
    }
}
