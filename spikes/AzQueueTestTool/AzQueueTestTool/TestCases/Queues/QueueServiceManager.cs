using AzQueueTestTool.TestCases.Rules;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using CSE.Automation.Model;
using CSE.Automation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public void GenerateMessageForRulesAsync(List<IRuleSet> ruleSetsList)
        {
            List<string> runLog = new List<string>();
            List<Task> queueTasks = new List<Task>();
            foreach (var ruleSet in ruleSetsList)
            {
                if (ruleSet.ServicePrincipals == null || ruleSet.ServicePrincipals.Count == 0)
                    continue;


                runLog.Add($"{ruleSet.GetType().Name}: {string.Join(';', ruleSet.ServicePrincipals.Select(x => new { name = x.DisplayName, x.Id }).ToList())}");

                ConsoleHelper.UpdateConsole($"Sending [{ruleSet.GetType().Name}] messages to Queue...");

                foreach (var sp in ruleSet.ServicePrincipals)
                {

                    var servicePrincipal = new ServicePrincipalModel()
                    {
                        Id = sp.Id,
                        AppId = sp.AppId,
                        DisplayName = sp.DisplayName,
                        Notes = sp.Notes,
                        Owners = ruleSet.HasOwners ? ruleSet.AADUsers.Select(x => x.DisplayName).ToList() : null
                    };

                    var myMessage = new QueueMessage<ServicePrincipalModel>()
                    {
                        QueueMessageType = QueueMessageType.Data,
                        Document = servicePrincipal,
                        Attempt = 0
                    };

                    Task queueTask = Task.Run(() => _azureQueueService.Send(myMessage, 3).ConfigureAwait(false));

                    queueTasks.Add(queueTask);


                }
            }

            Task.WaitAll(queueTasks.ToArray());

            string logFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmmss") + "_Execution.log");
            File.WriteAllLines(logFileName, runLog);
            Task.Delay(500);
            Process.Start("notepad.exe", logFileName);
        }

      
        void IDisposable.Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}
