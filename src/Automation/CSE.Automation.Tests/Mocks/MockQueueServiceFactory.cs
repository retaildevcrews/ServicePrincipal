using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class MockQueueServiceFactory<TEntity> : IQueueServiceFactory
    {
        private AzureQueueServiceMock<TEntity> _queue = null;

        public IAzureQueueService Create(string connectionString, string queueName)
        {
            return _queue ??= new AzureQueueServiceMock<TEntity>();
        }
    }
}
