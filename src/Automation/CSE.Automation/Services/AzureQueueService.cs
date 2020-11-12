﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Storage.Queues;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CSE.Automation.Services
{
    public class AzureQueueService : IAzureQueueService
    {
        private readonly QueueClient queueClient;

        public AzureQueueService(string connectionString, string queueName)
        {
            queueClient = new QueueClient(connectionString, queueName);

            if (!queueClient.Exists())
            {
                Console.WriteLine($"Queue {queueName} doesn't exist"); // TODO Change this to log

                // TODO end gracefully
            }
        }

        public async Task Send(QueueMessage message, int visibilityDelay = 0)
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
                    await queueClient
                        .SendMessageAsync(Base64Encode(payload), TimeSpan.FromSeconds(visibilityDelay))
                        .ConfigureAwait(false);
                    messageSent = true;
                }
                catch (Azure.RequestFailedException e)
                {
                    Console.WriteLine($"Cannot send message to queue {queueClient.Name} - Attempt:{numOfAttempts} - Message:{e.Message}"); // TODO Change this to log
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
