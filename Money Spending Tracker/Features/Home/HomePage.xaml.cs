using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Home;

public partial class HomePage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public HomePage(DatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;
        Loaded += HomePage_Loaded;
    }

    private async void HomePage_Loaded(object? sender, EventArgs e)
    {
        var viewModel = new HomeViewModel(_databaseService);
        BindingContext = viewModel;

        await viewModel.StartAsync();
    }
}
