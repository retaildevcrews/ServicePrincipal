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
        public TestCaseManager(QueueSettings queueSettings)
        {
            _queueSettings = queueSettings;
        }

        internal void Start()
        {
            var availableServicePrincipals = GetTargetServicePrincipals();// assures SP objects exist
            GenerateMessagesForAllRules(availableServicePrincipals);
        }

        internal void Cleanup()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ServicePrincipalManager.DeleteServicePrincipals();
            }
        }

        private void GenerateMessagesForAllRules(List<ServicePrincipal> availableServicePrincipals)
        {
            using (RulesManager rulesManager = new RulesManager(availableServicePrincipals, _spSettings.NumberOfSPObjectsToCreatePerTestCase))
            {
                rulesManager.ExecuteAllRules(_spSettings.TargetTestCaseList);

                using (var queueServiceManager = new QueueServiceManager("evaluate", _queueSettings.StorageConnectionString))
                {
                    queueServiceManager.GenerateMessageForRulesAsync(rulesManager.RuleSetsList);
                    
                }
            }
        }

      
        private List<ServicePrincipal> GetTargetServicePrincipals()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                return ServicePrincipalManager.GetOrCreateServicePrincipals();
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
