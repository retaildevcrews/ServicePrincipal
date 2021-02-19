using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Model;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ConfigurationResults;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    abstract class DiscoverSpStateDefinitionBase : IDiscoverSpStateDefinition
    {
        public TestCaseCollection.TestCase TestCaseID { get; }

        public  IConfigurationRoot Config { get; }

        public string DisplayNamePatternFilter { get;  }

        protected GraphDeltaProcessorHelper GraphDeltaProcessorHelper { get; }



        public DiscoverSpStateDefinitionBase(IConfigurationRoot config, TestCase testCase, GraphDeltaProcessorHelper graphDeltaProcessorHelper) 
        {

            TestCaseID = testCase;
            Config = config;
            DisplayNamePatternFilter = Config["displayNamePatternFilter"];

            GraphDeltaProcessorHelper = graphDeltaProcessorHelper;
        }

        protected bool DeleteDynamicCreatedTestServicePrincipals()
        {
            string spTestRemoveAttibuteToDelete = $"{DisplayNamePatternFilter}{TestCaseCollection.TestRemovedAttributeSuffix}";
            string spTestNewUserToDelete = $"{DisplayNamePatternFilter}{TestCaseCollection.TestNewUserSuffix}";

            List<Task> tasks = new List<Task>
            {
                Task.Run(() => DeleteServicePrincipal(spTestRemoveAttibuteToDelete)),
                Task.Run(() => DeleteServicePrincipal(spTestNewUserToDelete))
            };

            Task.WaitAll(tasks.ToArray());

            return true;
        }

        internal void DeleteServicePrincipal(string servicePrincipalToDelete)
        {
            using ServicePrincipalHelper servicePrincipalHelper = new ServicePrincipalHelper();
            servicePrincipalHelper.DeleteServicePrincipal(servicePrincipalToDelete);
        }


        protected bool RunFullSeedDiscovery()
        {
            using var testCaseCollection = new DiscoverTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC1;

            using var activityContext = GraphDeltaProcessorHelper.ActivityServiceInstance.CreateContext($"Nested execution Integration Test - Test Case [{thisTestCase}] ", withTracking: true);

            GraphDeltaProcessorHelper.DeleteDynamicCreatedServicePrincipals = false;
            string mainTestCaseConfigId  = GraphDeltaProcessorHelper.MainTestCaseConfigId;

            using var inputGenerator = new DiscoverInputGenerator(GraphDeltaProcessorHelper.ConfigInstance, GraphDeltaProcessorHelper.GraphHelperSettingsInstance, testCaseCollection, thisTestCase, mainTestCaseConfigId, GraphDeltaProcessorHelper);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(DiscoveryMode.FullSeed, "HTTP", activityContext));

            // Creating a ConfigurationValidationManager instance will clear out the Config.lock 
            using var configurationValidationManager = new ConfigurationValidationManager(inputGenerator, GraphDeltaProcessorHelper.ConfigRepositoryInstance, activityContext);

            Task thisTask = Task.Run (() => GraphDeltaProcessorHelper.GraphDeltaProcessorInstance.Discover(cloudQueueMessage, GraphDeltaProcessorHelper.GraphLoggerInstance));
            thisTask.Wait();

            return true;

        }
       
        abstract public bool Validate();
    }
}
