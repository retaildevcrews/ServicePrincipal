using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class NoopAzureQueueService : IAzureQueueService
    {
        public async Task Send(QueueMessage message, int visibilityDelay = 0)
        {
            await Task.CompletedTask;
        }
    }
}
