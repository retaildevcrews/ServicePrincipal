using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IDeltaProcessor
    {
        public int VisibilityDelayGapSeconds { get; }
        public int QueueRecordProcessThreshold { get; }

        public Task<int> ProcessDeltas();
    }
}
