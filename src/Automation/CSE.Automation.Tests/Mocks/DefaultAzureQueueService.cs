using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class DefaultAzureQueueService<TEntity> : IAzureQueueService 
    {
        public List<QueueMessage<TEntity>> Data { get; set; } = new List<QueueMessage<TEntity>>();

        public async Task Send(QueueMessage message, int visibilityDelay = 0)
        {
            this.Data.Add(message as QueueMessage<TEntity>);
            await Task.CompletedTask;
        }
    }
}
