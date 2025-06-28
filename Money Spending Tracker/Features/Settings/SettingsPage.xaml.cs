using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(DatabaseService databaseService)
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel(databaseService);
    }
}