using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzQueueTestTool
{
    class QueueManager : IQueueManager, IDisposable
    {
        private readonly QueueClient _queue;
        private readonly string _queueName;
        private readonly string _messageBase;

        public string StatusMessage { get; set; }

        public QueueManager(string queueName, string messageBase,  string storageConnectionString )
        {
            _messageBase = messageBase;
            _queueName = queueName.Trim();
            
            _queue = new QueueClient(storageConnectionString, _queueName);

            _queue.Create();

        }

        
        public void AddMessage(string messageText)
        {
            _queue.SendMessage(messageText);
        }

        public void AddBaseMessages(int messageCount=500)
        {
            Parallel.For(1, messageCount + 1, (i, state) =>
            {
                string uniqueMessage = $"{_queueName.ToUpper()} - {_messageBase.AddRandomString()} - {i}";
                AddMessage(uniqueMessage);
                UpdateConsole($"{i} messages sent to Queue: '{_queueName}'");
            });

            StatusMessage = ($"{messageCount} sent to queue '{_queueName}'");
        }


        public void RetrieveMessage(int maxMessages = 50)
        {
            foreach (QueueMessage message in _queue.ReceiveMessages(maxMessages: maxMessages).Value)
            {
                // "Process" the message
                Console.WriteLine($"Message: {message.MessageText}");

                // delete message from queue
                _queue.DeleteMessage(message.MessageId, message.PopReceipt);
            }
        }

        public void UpdateConsole(string message)
        {
            Console.Write(string.Format("\r{0}", "".PadLeft(Console.CursorLeft, ' ')));
            Console.Write(string.Format("\r{0}", message));
            
        }

        void IDisposable.Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}
