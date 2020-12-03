using AzQueueTestTool.TestCases.Queues;
using AzQueueTestTool.TestCases.Rules;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzQueueTestTool.TestCases
{
    internal class TestCaseManager : IDisposable
    {
        private readonly QueueSettings _queueSettings;

        private readonly ServicePrincipalSettings _spSettings = new ServicePrincipalSettings();

        public string LogFileName { get; private set; }

        public TestCaseManager(QueueSettings queueSettings)
        {
            _queueSettings = queueSettings;
        }

        internal void Start()
        {
            List<Task> queryObjects = new List<Task>();
          
            Task<List<ServicePrincipal>> getSPsTask = Task.Run(() => GetTargetServicePrincipals());
            queryObjects.Add(getSPsTask);

            Task<List<User>> getUserTask = Task.Run(() => GetTargetAADUsers());
            queryObjects.Add(getUserTask);

            Task.WaitAll(queryObjects.ToArray());

            GenerateMessagesForAllRules(getSPsTask.Result, getUserTask.Result);
        }

        internal void Cleanup()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ServicePrincipalManager.DeleteServicePrincipals();
            }
        }

        private void GenerateMessagesForAllRules(List<ServicePrincipal> availableServicePrincipals, List<User> availableUsers)
        {
            using (RulesManager rulesManager = new RulesManager(availableServicePrincipals, availableUsers, _spSettings))
            {
                rulesManager.ExecuteAllRules();

                if (_queueSettings.PushMessagesToQueueEvaluate)
                {
                    using (var queueServiceManager = new QueueServiceManager("evaluate", _queueSettings.StorageConnectionString))
                    {
                        queueServiceManager.GenerateMessageForRulesAsync(rulesManager.RuleSetsList);

                        LogFileName = queueServiceManager.LogFileName;
                    }
                }
            }
        }

      
        private List<ServicePrincipal> GetTargetServicePrincipals()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ConsoleHelper.UpdateConsole($"Getting Service Principal Objects...");
                return ServicePrincipalManager.GetOrCreateServicePrincipals();
            }

        }

        private List<User> GetTargetAADUsers()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ConsoleHelper.UpdateConsole($"Getting User Objects...");
                return ServicePrincipalManager.GetOrCreateUsers();
            }

        }

        internal void DeleteServicePrincipals()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ServicePrincipalManager.DeleteServicePrincipals();
            }
        }


        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
