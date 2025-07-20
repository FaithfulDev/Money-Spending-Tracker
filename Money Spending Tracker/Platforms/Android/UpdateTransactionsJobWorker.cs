using Android.Content;
using AndroidX.Work;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.BackgroundJob;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Storage;
using Money_Spending_Tracker.Features.TransactionData;
using System.Diagnostics;

namespace Money_Spending_Tracker.Platforms.Android;

public class UpdateTransactionsJobWorker : Worker
{
    public UpdateTransactionsJobWorker(Context context, WorkerParameters workerParams)
        : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        return DoWorkAsync().GetAwaiter().GetResult();
    }

    private static async Task<Result> DoWorkAsync()
    {
        var dbPassword = await SecureStorage.GetAsync(StorageKeys.DB_PASSWORD);

        if (string.IsNullOrEmpty(dbPassword))
        {
            Debug.WriteLine("Database password is not set. Cannot unlock the database.");

            // We don't want the work manager to retry this job if the password is not set.
            return Result.InvokeSuccess();
        }

        // "o" = ISO 8601 format
        var startedAt = DateTime.UtcNow.ToString("o");

        Debug.WriteLine("Daily job worker is running...");

        var transactionDataService = MauiServiceProvider.Current!.GetService<ITransactionDataService>();
        var databaseService = MauiServiceProvider.Current!.GetService<DatabaseService>();
        string? errorMessage = null;
        string? errorStackTrace = null;

        try
        {
            await databaseService!.UnlockAndInitializeAsync(dbPassword);
            await transactionDataService!.UpdateTransactionsAndCacheAsync();

            Debug.WriteLine("Daily job worker completed successfully.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in daily job worker: {ex.Message}");
            errorMessage = ex.Message;
            errorStackTrace = ex.StackTrace;
        }

        var finishedAt = DateTime.UtcNow.ToString("o");

        var output = new AndroidX.Work.Data.Builder()
            .PutString("startedAt", startedAt)
            .PutString("finishedAt", finishedAt)
            .PutString("errorMessage", errorMessage)
            .PutString("errorStackTrace", errorStackTrace)
            .Build();

        await CreateJobLogAsync(startedAt, finishedAt, errorMessage, errorStackTrace);

        return Result.InvokeSuccess(output);
    }

    private static async Task CreateJobLogAsync(string startedAt, string finishedAt, string? errorMessage, string? errorStackTrace)
    {
        using var dbContext = MauiServiceProvider.Current!.GetService<DatabaseService>()!.CreateDbContext();

        var jobState = string.IsNullOrEmpty(errorMessage) ? JobState.SUCCEEDED : JobState.FAILED;

        var jobLog = new JobLog(
            startedAt: startedAt,
            finishedAt: finishedAt,
            errorMessage: errorMessage,
            errorStackTrace: errorStackTrace,
            state: jobState.ToString()
        );

        dbContext.JobLogs.Add(jobLog);

        await dbContext.SaveChangesAsync();
    }
}
