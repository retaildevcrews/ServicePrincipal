using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;


namespace CSE.Automation.Services
{
    public class AzureQueueService : IAzureQueueService
    {
        private readonly QueueClient _queueClient;
        
        public AzureQueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            
            //if (!_queueClient.Exists())
            //{
            //    //TODO Handle the case where the queueName doesn't exist

            //}
        }

        public async Task Send(QueueMessage message,int visibilityDelay)
        {
            if (_queueClient.Exists())
            {
                await _queueClient
                    .SendMessageAsync(JsonConvert.SerializeObject(message),TimeSpan.FromSeconds(visibilityDelay))
                    .ConfigureAwait(true);
            }
            //else
            //{
            //    //TODO Handle the case where the queueName doesn't exist
            //}
        }
    }
}
