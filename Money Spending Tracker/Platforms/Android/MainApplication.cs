using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Runtime;

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
            var widgetComponent = new ComponentName(context, Java.Lang.Class.FromType(typeof(MonthlyBudgetWidgetProvider)));

            int[] widgetIds = widgetManager!.GetAppWidgetIds(widgetComponent)!;

            Intent updateIntent = new(context, typeof(MonthlyBudgetWidgetProvider));
            updateIntent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            updateIntent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, widgetIds);

            context.SendBroadcast(updateIntent);
        }
    }
}
