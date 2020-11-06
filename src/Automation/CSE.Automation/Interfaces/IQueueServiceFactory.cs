using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Interfaces
{
    public interface IQueueServiceFactory
    {
        IAzureQueueService Create(string connectionString, string queueName);
    }
}
