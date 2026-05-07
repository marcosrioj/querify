using Hangfire.Common;
using Hangfire.States;

namespace Querify.Common.Infrastructure.Hangfire.Attributes;

public class QueueToCurrentServerOnlyAttribute : JobFilterAttribute, IElectStateFilter
{
    public string QueueName
    {
        get
        {
            var queueName = Environment.MachineName;
            queueName = new string(queueName.Where(char.IsLetterOrDigit).ToArray()).ToLower();
            return queueName;
        }
    }

    public void OnStateElection(ElectStateContext context)
    {
        var enqueuedState = context.CandidateState as EnqueuedState;
        if (enqueuedState != null)
        {
            enqueuedState.Queue = QueueName;
        }
    }
}