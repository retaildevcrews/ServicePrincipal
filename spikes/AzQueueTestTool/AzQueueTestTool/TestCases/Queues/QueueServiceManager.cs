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

        public string LogFileName { get; set; }
        public string StatusMessage { get; set; }

        public QueueServiceManager(string queueName,  string storageConnectionString )
        {
            _azureQueueService = new AzureQueueService(storageConnectionString, queueName.Trim());
        }

        public void GenerateMessageForRulesAsync(List<IRuleSet> ruleSetsList)
        {
            List<string> logger = new List<string>();
            List<Task> queueTasks = new List<Task>();

            foreach (var ruleSet in ruleSetsList)
            {
                if (ruleSet.ServicePrincipals == null || ruleSet.ServicePrincipals.Count == 0)
                    continue;

                StringBuilder sbLog = new StringBuilder();

                BuildLogEntry(ruleSet, sbLog);

                logger.Add(sbLog.ToString());

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

            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

            LogFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_Execution.log");
            File.WriteAllLines(LogFileName, logger);
            Task.Delay(500);
        }

        private void BuildLogEntry(IRuleSet ruleSet, StringBuilder sbLog)
        {
            sbLog.AppendLine($"*** {ruleSet.GetType().Name} ***");
            sbLog.AppendLine($"-> ServicePrincipals{Environment.NewLine}{string.Join(Environment.NewLine, ruleSet.ServicePrincipals.Select(x => new { name = x.DisplayName, x.Id}).ToList())}");

            if (ruleSet.HasOwners)
                sbLog.AppendLine($"-> Owners{Environment.NewLine}{string.Join(Environment.NewLine, ruleSet.AADUsers.Select(x => new { name = x.DisplayName, x.Id }).ToList())}");
            else
                sbLog.AppendLine($"-> Owners{Environment.NewLine} <<none>>");
        }

        void IDisposable.Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}
