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

            if (!_queueClient.Exists())
            {
                Console.WriteLine($"Queue {queueName} doesn't exist"); //TODO Change this to log
                //TODO end gracefully

            }
        }

        public async Task Send(QueueMessage message, int visibilityDelay)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            bool messageSent = false;
            int numOfAttempts = 1;
            
            while (!messageSent && numOfAttempts <= 3)
            {
                try
                {
                    message.Attempt = numOfAttempts;

                    await _queueClient
                        .SendMessageAsync(JsonConvert.SerializeObject(message), TimeSpan.FromSeconds(visibilityDelay))
                        .ConfigureAwait(false);
                    messageSent = true;
                }
                catch (Azure.RequestFailedException e)
                {
                    Console.WriteLine($"Cannot send message to queue {_queueClient.Name} - Attempt:{numOfAttempts} - Message:{e.Message}"); //TODO Change this to log
                }
                numOfAttempts++;
            }
        }
    }
}
