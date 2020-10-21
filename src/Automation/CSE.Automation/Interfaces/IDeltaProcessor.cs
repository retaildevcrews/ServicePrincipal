using CSE.Automation.Model;
using System;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IDeltaProcessor
    {
        int VisibilityDelayGapSeconds { get; }
        int QueueRecordProcessThreshold { get; }

        Task<int> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
    }
}
