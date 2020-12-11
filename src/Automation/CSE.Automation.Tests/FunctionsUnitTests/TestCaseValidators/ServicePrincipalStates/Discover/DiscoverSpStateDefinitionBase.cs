using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
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
            //NullOutConfigDataLink();

            using var testCaseCollection = new DiscoverTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC1;

            using var activityContext = GraphDeltaProcessorHelper.ActivityServiceInstance.CreateContext($"Nested execution Integration Test - Test Case [{thisTestCase}] ", withTracking: true);

            GraphDeltaProcessorHelper.DeleteDynamicCreatedServicePrincipals = false;


            using var inputGenerator = new DiscoverInputGenerator(GraphDeltaProcessorHelper.ConfigInstance, testCaseCollection, thisTestCase, GraphDeltaProcessorHelper);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(DiscoveryMode.FullSeed, "HTTP", activityContext));

            Task thisTask = Task.Run (() => GraphDeltaProcessorHelper.GraphDeltaProcessorInstance.Discover(cloudQueueMessage, GraphDeltaProcessorHelper.GraphLoggerInstance));
            thisTask.Wait();

            return true;

        }
        private bool NullOutConfigDataLink()
        {
            ProcessorConfiguration configuration = GraphDeltaProcessorHelper.ConfigRepositoryInstance.GetByIdAsync(GraphDeltaProcessorHelper.ConfigInstance["configId"], ProcessorType.ServicePrincipal.ToString()).GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(configuration.DeltaLink))
            {
                configuration.DeltaLink = string.Empty;

                configuration = GraphDeltaProcessorHelper.ConfigRepositoryInstance.UpsertDocumentAsync(configuration).GetAwaiter().GetResult();
            }

            return configuration != null;
        }
        abstract public bool Validate();
    }
}
