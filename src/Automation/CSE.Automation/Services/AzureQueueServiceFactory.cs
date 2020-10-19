using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Interfaces;

namespace CSE.Automation.Services
{
    internal class AzureQueueServiceFactory : IQueueServiceFactory
    {
        public IAzureQueueService Create(string connectionString, string queueName)
        {
            return new AzureQueueService(connectionString, queueName);
        }
    }
}
