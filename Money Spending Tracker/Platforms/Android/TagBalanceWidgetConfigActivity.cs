using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Storage;
using AndroidWidgetButton = Android.Widget.Button;

namespace Money_Spending_Tracker.Platforms.Android;

[Activity(
    Name = "de.faithfuldevapps.moneyspendingtracker.TagBalanceWidgetConfigActivity",
    Label = "Tag Balance",
    Theme = "@style/Theme.AppCompat.Light.Dialog",
    Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_CONFIGURE" })]
public class TagBalanceWidgetConfigActivity : AppCompatActivity
{
    private int _appWidgetId = AppWidgetManager.InvalidAppwidgetId;
    private Spinner? _tagSpinner;
    private List<TagItem> _tags = [];

    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set result to CANCELED initially
        SetResult(Result.Canceled);

        // Get widget ID from intent
        _appWidgetId = Intent!.GetIntExtra(
            AppWidgetManager.ExtraAppwidgetId,
            AppWidgetManager.InvalidAppwidgetId);

        if (_appWidgetId == AppWidgetManager.InvalidAppwidgetId)
        {
            Finish();
            return;
        }

        _tags = await GetTagsFromDatabase();

        CreateUi();
    }

    private void CreateUi()
    {
        var adapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem,
            [.. _tags.Select(t => t.Name)]);

        adapter.SetDropDownViewResource(
            global::Android.Resource.Layout.SimpleSpinnerDropDownItem);

        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical,
        };

        layout.SetPadding(16, 16, 16, 16);

        _tagSpinner = new Spinner(this)
        {
            Adapter = adapter
        };

        var spinnerParams = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MatchParent,
            LinearLayout.LayoutParams.WrapContent);

        spinnerParams.SetMargins(16, 16, 16, 16);

        var confirmButton = new AndroidWidgetButton(this) { Text = "Confirm" };
        confirmButton.Click += ConfirmButton_Click;

        var buttonParams = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MatchParent,
            LinearLayout.LayoutParams.WrapContent);

        buttonParams.SetMargins(16, 16, 16, 16);

        layout.AddView(_tagSpinner, spinnerParams);
        layout.AddView(confirmButton, buttonParams);
        SetContentView(layout);
    }

    private void ConfirmButton_Click(object? sender, System.EventArgs e)
    {
        int selectedPosition = _tagSpinner!.SelectedItemPosition;

        if (selectedPosition < 0 || selectedPosition >= _tags.Count)
        {
            Toast.MakeText(this, "Please select a tag.", ToastLength.Short)?.Show();
            return;
        }

        var selectedTag = _tags[selectedPosition];

        // Save the tag preference for this widget
        AppCache.SetWidget(_appWidgetId, selectedTag.Id, selectedTag.Name);

        // Update the widget
        MainApplication.TriggerWidgetUpdate();

        // Return success
        var resultValue = new Intent();
        resultValue.PutExtra(AppWidgetManager.ExtraAppwidgetId, _appWidgetId);
        SetResult(Result.Ok, resultValue);
        Finish();
    }

    private static async Task<List<TagItem>> GetTagsFromDatabase()
    {
        var databaseService = MauiServiceProvider.Current!.GetRequiredService<DatabaseService>();
        await databaseService.UnlockAndInitializeAsync();

        using var dbContext = databaseService.CreateDbContext();

        return await dbContext.Tags
            .AsNoTracking()
            .Select(t => new TagItem(t.Id, t.Name))
            .ToListAsync();
    }

    private sealed record TagItem(int Id, string Name);
}
