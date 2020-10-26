using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    public interface IDeltaProcessor
    {
        int VisibilityDelayGapSeconds { get; }
        int QueueRecordProcessThreshold { get; }

        Task<int> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
    }
}
