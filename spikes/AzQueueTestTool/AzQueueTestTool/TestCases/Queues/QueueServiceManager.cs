using AzQueueTestTool.TestCases.Rules;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using CSE.Automation.Model;
using CSE.Automation.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzQueueTestTool.TestCases.Queues
{
    internal class QueueServiceManager : IQueueManager, IDisposable
    {
        private readonly AzureQueueService _azureQueueService;

        public string StatusMessage { get; set; }

        public QueueServiceManager(string queueName,  string storageConnectionString )
        {
            _azureQueueService = new AzureQueueService(storageConnectionString, queueName.Trim());
        }

        public async Task GenerateMessageForRulesAsync(List<IRuleSet> ruleSetsList)
        {
            foreach(var ruleSet in ruleSetsList)
            {
                if (ruleSet.ServicePrincipals?.Count == 0)
                    continue;

                foreach(var sp in ruleSet.ServicePrincipals)
                //Parallel.ForEach(ruleSet.ServicePrincipals, async sp =>
                {
                    var servicePrincipal = new ServicePrincipalModel()
                    {
                        Id = sp.Id,
                        AppId = sp.AppId,
                        DisplayName = sp.DisplayName,
                        Notes = sp.Notes,
                    };

                    var myMessage = new QueueMessage<ServicePrincipalModel>()
                    {
                        QueueMessageType = QueueMessageType.Data,
                        Document = servicePrincipal,
                        Attempt = 0
                    };

                    await _azureQueueService.Send(myMessage, 3).ConfigureAwait(false);
                }
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
