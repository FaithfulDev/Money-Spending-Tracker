using Android.Runtime;
using AndroidX.Work;
using Money_Spending_Tracker.Features.BackgroundJob;
using Money_Spending_Tracker.Features.Database;
using System.Globalization;

namespace Money_Spending_Tracker.Platforms.Android
{
    internal class BackgroundService : IBackgroundService
    {
        private readonly DatabaseService _databaseService;

        public BackgroundService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public List<BackgroundJobModel> GetJobInfo()
        {
            var workInfos = GetWorkInfos();
            List<BackgroundJobModel> jobInfos = [];

            // Everything that is scheduled with the WorkManager
            foreach (var info in workInfos)
            {
                //Check if there is a next scheduled time. If it is long.MaxValue, there is no next scheduled time.
                DateTime? scheduled = null;
                if (info.NextScheduleTimeMillis != long.MaxValue)
                {
                    scheduled = DateTimeOffset.FromUnixTimeMilliseconds(info.NextScheduleTimeMillis).ToLocalTime().DateTime;
                }

                var jobInfo = new BackgroundJobModel(
                    scheduled: scheduled,
                    startedAt: ParseDateTime(info.OutputData.GetString(OutputDataParameter.STARTED_AT)),
                    finishedAt: ParseDateTime(info.OutputData.GetString(OutputDataParameter.FINISHED_AT)),
                    errorMessage: info.OutputData.GetString(OutputDataParameter.ERROR_MESSAGE),
                    errorStackTrace: info.OutputData.GetString(OutputDataParameter.ERROR_STACK_TRACE),
                    state: ConvertToJobState(info.GetState())
                );

                jobInfos.Add(jobInfo);
            }

            var dbContext = _databaseService.CreateDbContext();

            // Everything that is logged in the database
            foreach (var jobLog in dbContext.JobLogs.OrderByDescending(l => l.StartedAt).Take(30))
            {
                var jobInfo = new BackgroundJobModel(
                    scheduled: null,
                    startedAt: ParseDateTime(jobLog.StartedAt),
                    finishedAt: ParseDateTime(jobLog.FinishedAt),
                    errorMessage: jobLog.ErrorMessage,
                    errorStackTrace: jobLog.ErrorStackTrace,
                    state: Enum.TryParse<JobState>(jobLog.State, out var state) ? state : JobState.UNKNOWN
                );
                jobInfos.Add(jobInfo);
            }

            return jobInfos;
        }

        private static DateTime? ParseDateTime(string? dateTimeString)
        {
            if (DateTime.TryParseExact(dateTimeString, "o", CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal, out DateTime result))
            {
                return result.ToLocalTime();
            }

            return null;
        }

        private static JobState ConvertToJobState(WorkInfo.State state)
        {
            if (state == WorkInfo.State.Blocked)
            {
                return JobState.BLOCKED;
            }
            else if (state == WorkInfo.State.Cancelled)
            {
                return JobState.CANCELED;
            }
            else if (state == WorkInfo.State.Enqueued)
            {
                return JobState.ENQUEUED;
            }
            else if (state == WorkInfo.State.Failed)
            {
                return JobState.FAILED;
            }
            else if (state == WorkInfo.State.Running)
            {
                return JobState.RUNNING;
            }
            else if (state == WorkInfo.State.Succeeded)
            {
                return JobState.SUCCEEDED;
            }

            return JobState.UNKNOWN;
        }

        private static List<WorkInfo> GetWorkInfos()
        {
            var workInfosObject = WorkManager.GetInstance(Platform.CurrentActivity!.ApplicationContext!)
                .GetWorkInfosByTag(nameof(UpdateTransactionsJobWorker))
                .Get(); // returns Java.Lang.Object

            var workInfosJavaList = workInfosObject as JavaList;

            var workInfos = new List<WorkInfo>();

            for (int i = 0; i < workInfosJavaList?.Size(); i++)
            {
                var item = workInfosJavaList.Get(i);
                if (item is WorkInfo info)
                {
                    workInfos.Add(info);
                }
            }

            return workInfos;
        }
    }
}
