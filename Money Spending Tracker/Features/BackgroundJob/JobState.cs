namespace Money_Spending_Tracker.Features.BackgroundJob;

public enum JobState
{
    UNKNOWN = 0,
    BLOCKED = 1,
    CANCELED = 2,
    ENQUEUED = 3,
    FAILED = 4,
    RUNNING = 5,
    SUCCEEDED = 6,
}
