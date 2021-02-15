using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class DefaultQueueServiceFactory<TEntity> : IQueueServiceFactory
    {
        private DefaultAzureQueueService<TEntity> _queue = null;

        public IAzureQueueService Create(string connectionString, string queueName)
        {
            return _queue ??= new DefaultAzureQueueService<TEntity>();
        }
    }
}
