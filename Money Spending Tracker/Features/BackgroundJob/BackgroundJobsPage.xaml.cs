namespace Money_Spending_Tracker.Features.BackgroundJob;

public partial class BackgroundJobsPage : ContentPage
{
    public BackgroundJobsPage(IBackgroundService backgroundService)
    {
        InitializeComponent();
        BindingContext = new BackgroundJobsViewModel(backgroundService);

        NavigatedTo += BackgroundJobsPage_NavigatedTo;
    }

    private void BackgroundJobsPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        var viewModel = BindingContext as BackgroundJobsViewModel;
        viewModel?.Start();
    }
}