using CSE.Automation.Interfaces;

namespace CSE.Automation.Tests.Mocks
{
    internal class NoopQueueServiceFactory : IQueueServiceFactory
    {
        public IAzureQueueService Create(string connectionString, string queueName)
        {
            return new NoopAzureQueueService();
        }
    }
}
