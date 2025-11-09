using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Tags;

public partial class TagListPage : ContentPage
{
    private readonly TagListViewModel _viewModel;

    public TagListPage(DatabaseService databaseService)
    {
        InitializeComponent();

        _viewModel = new TagListViewModel(databaseService);
        BindingContext = _viewModel;

        NavigatedTo += TagListPage_NavigatedTo;
    }

    private void TagListPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        _viewModel.Start();
    }
}