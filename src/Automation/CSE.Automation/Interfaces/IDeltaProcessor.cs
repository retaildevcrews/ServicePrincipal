using CSE.Automation.Graph;
using CSE.Automation.Model;
using System;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IDeltaProcessor
    {
        int VisibilityDelayGapSeconds { get; }
        int QueueRecordProcessThreshold { get; }

        Task<GraphOperationMetrics> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
    }
}
