namespace Querify.Common.Infrastructure.Hangfire.Abstractions;

public interface IHangFireJobService
{
    void PurgeAllQueues();

    void PurgeQueue(string queueName);

    /// <summary>
    /// PS: not tested yet       
    /// </summary>
    /// <param name="newJobIds"></param>
    void ResetRecurringJobs(List<string> newJobIds);
}