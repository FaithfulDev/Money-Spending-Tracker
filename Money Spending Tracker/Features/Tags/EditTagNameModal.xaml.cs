using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Tags;

[QueryProperty(nameof(TagId), nameof(TagId))]
public partial class EditTagNameModal : ContentPage
{
    public int TagId { get; set; }

    public EditTagNameModal(DatabaseService databaseService)
    {
        InitializeComponent();
        BindingContext = new EditTagNameViewModel(databaseService);

        Loaded += EditTagNameModal_Loaded;
    }

    private async void EditTagNameModal_Loaded(object? sender, EventArgs e)
    {
        var viewModel = (EditTagNameViewModel)BindingContext;
        await viewModel.StartAsync(TagId);
    }
}
