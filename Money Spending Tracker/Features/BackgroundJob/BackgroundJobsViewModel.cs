using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.BackgroundJob;

internal partial class BackgroundJobsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<BackgroundJobModel> _jobs = [];

    private readonly IBackgroundService _backgroundService;

    public BackgroundJobsViewModel(IBackgroundService backgroundService)
    {
        _backgroundService = backgroundService;
    }

    public void Start()
    {
        Jobs = _backgroundService.GetJobInfo().ToObservableCollection();
    }
}
