using CSE.Automation.Graph;
using CSE.Automation.Model;
using System;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    internal interface IDeltaProcessor
    {
        int VisibilityDelayGapSeconds { get; }
        int QueueRecordProcessThreshold { get; }

        Task<ActivityHistory> GetActivityStatus(ActivityContext context, string activityId);
        Task RequestDiscovery(ActivityContext context, DiscoveryMode discoveryMode);

        Task<GraphOperationMetrics> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
        Task Lock();
        Task Unlock();
    }
}
