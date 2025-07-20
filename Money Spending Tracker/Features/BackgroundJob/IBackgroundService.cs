namespace Money_Spending_Tracker.Features.BackgroundJob;

public interface IBackgroundService
{
    public List<BackgroundJobModel> GetJobInfo();
}
