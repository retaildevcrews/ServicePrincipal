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

        public async Task Send(QueueMessage message, int visibilityDelay=0)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            bool messageSent = false;
            int numOfAttempts = 1;

            while (!messageSent && numOfAttempts <= 3)
            {
                try
                {
                    message.Attempt = numOfAttempts;
                    var payload = JsonConvert.SerializeObject(message);
                    await _queueClient
                        .SendMessageAsync(Base64Encode(payload), TimeSpan.FromSeconds(visibilityDelay))
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
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
