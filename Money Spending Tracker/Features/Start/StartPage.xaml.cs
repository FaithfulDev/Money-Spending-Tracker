using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Start;

public partial class StartPage : ContentPage
{
    private readonly StartViewModel _startViewModel;

    public StartPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _startViewModel = new StartViewModel(databaseService);

        BindingContext = _startViewModel;

        Loaded += StartPage_Loaded;
    }

    private async void StartPage_Loaded(object? sender, EventArgs e)
    {
        await _startViewModel.Start();
    }
}