using Hangfire;
using Hangfire.Storage.Monitoring;
using Querify.Common.Infrastructure.Hangfire.Abstractions;

namespace Querify.Common.Infrastructure.Hangfire.Services;

public class HangFireJobService : IHangFireJobService
{
    public void PurgeAllQueues()
    {
        var monitor = JobStorage.Current.GetMonitoringApi();
        foreach (QueueWithTopEnqueuedJobsDto queue in monitor.Queues())
        {
            PurgeQueue(queue.Name);
        }
    }

    public void PurgeQueue(string queueName)
    {
        var toDelete = new List<string>();
        var monitor = JobStorage.Current.GetMonitoringApi();

        var queue = monitor.Queues().FirstOrDefault(x => x.Name == queueName);
        if (queue == null)
        {
            return;
        }

        for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
        {
            monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
                .ForEach(x => toDelete.Add(x.Key));
        }
        foreach (var jobId in toDelete)
        {
            BackgroundJob.Delete(jobId);
        }
    }


    public void ResetRecurringJobs(List<string> newJobIds)
    {
        using (var connection = JobStorage.Current.GetConnection())
        {
            var setKey = "recurring-jobs";
            var savedJobIds = connection.GetAllItemsFromSet(setKey);
            var missingJobsIds = savedJobIds.Except(newJobIds).ToList();

            foreach (var jobId in missingJobsIds)
            {
                RecurringJob.RemoveIfExists(jobId);
            }
        }
    }
}