namespace Money_Spending_Tracker.Data;

public class JobLog
{
    public int Id { get; set; }
    public string StartedAt { get; set; }
    public string FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
    public string State { get; set; }

    public JobLog(string startedAt, string finishedAt, string? errorMessage, string? errorStackTrace, string state)
    {
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        ErrorMessage = errorMessage;
        ErrorStackTrace = errorStackTrace;
        State = state;
    }
}
