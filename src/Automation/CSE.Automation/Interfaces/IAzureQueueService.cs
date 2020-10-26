using System.Threading.Tasks;
using CSE.Automation.Model;


namespace CSE.Automation.Interfaces
{
    public interface IAzureQueueService
    {
        public Task Send(QueueMessage message, int visibilityDelay);
    }
}
