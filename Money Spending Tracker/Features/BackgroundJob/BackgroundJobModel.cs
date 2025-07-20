namespace Money_Spending_Tracker.Features.BackgroundJob;

public class BackgroundJobModel
{
    public DateTime? Scheduled { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorStackTrace { get; set; }

    public JobState State { get; set; }

    public BackgroundJobModel(DateTime? scheduled, DateTime? startedAt, DateTime? finishedAt,
        string? errorMessage, string? errorStackTrace, JobState state)
    {
        Scheduled = scheduled;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        ErrorMessage = errorMessage;
        ErrorStackTrace = errorStackTrace;
        State = state;
    }
}
