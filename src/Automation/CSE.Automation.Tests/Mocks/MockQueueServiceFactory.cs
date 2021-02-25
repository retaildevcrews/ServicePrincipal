using System;
using System.Collections.Generic;
using CSE.Automation.Interfaces;
using CSE.Automation.Model.Commands;

namespace CSE.Automation.Tests.Mocks
{
    internal class MockQueueServiceFactory : IQueueServiceFactory
    {
        private readonly Dictionary<string, IAzureQueueService> queues = new Dictionary<string, IAzureQueueService>();
        private readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type>()
        {
            { "evaluate", typeof(AzureQueueServiceMock<ServicePrincipalEvaluateCommand>) },
            { "update", typeof(AzureQueueServiceMock<ServicePrincipalUpdateCommand>) },
        };

        public IAzureQueueService Create(string connectionString, string queueName)
        {
            if (queues.TryGetValue(queueName, out var queue) == false)
            {
                queue = queues[queueName] = Activator.CreateInstance(typeMap[queueName]) as IAzureQueueService;
            }

            return queue;
        }
    }
}
