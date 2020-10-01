using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IDeltaProcessor
    {
        public Task<int> ProcessDeltas();
    }
}