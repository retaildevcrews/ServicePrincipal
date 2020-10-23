using AzQueueTestTool.TestCases.Queues;
using AzQueueTestTool.TestCases.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzQueueTestTool.TestCases
{
    internal class TestCaseManager : IDisposable
    {
        private readonly QueueSettings _queueSettings;
        public TestCaseManager(QueueSettings queueSettings)
        {
            _queueSettings = queueSettings;
        }

        public void GenerateMessagesForAllRules()
        {
            using (RulesManager rulesManager = new RulesManager())
            {
                rulesManager.ExecuteAllRules();
                using (var queueServiceManager = new QueueServiceManager("evaluate", _queueSettings.StorageConnectionString))
                {
                    queueServiceManager.GenerateMessageForRules(rulesManager.RuleSetsList);
                }
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
