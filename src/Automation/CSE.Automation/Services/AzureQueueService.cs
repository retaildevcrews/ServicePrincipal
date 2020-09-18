using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Newtonsoft.Json;


namespace CSE.Automation.Services
{
    public class AzureQueueService : IAzureQueueService
    {
        private readonly QueueClient _queueClient;

        public AzureQueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExistsAsync(); //TODO decide if need to create if not exists already
        }

      
        public async Task Send(QueueMessage message,int visibilityDelay)
        {
            if (_queueClient.Exists())
            {
                await _queueClient
                    .SendMessageAsync(JsonConvert.SerializeObject(message),TimeSpan.FromSeconds(visibilityDelay))
                    .ConfigureAwait(true);
            }
            else
            {
                //TODO Handle the case where the queueName doesn't exist
            }
        }
    }
}
