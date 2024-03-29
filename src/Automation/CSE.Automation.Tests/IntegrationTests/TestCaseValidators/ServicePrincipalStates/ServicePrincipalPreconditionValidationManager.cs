﻿using System;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates.Discover;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates.Update;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates
{
    internal class ServicePrincipalPreconditionValidationManager : IDisposable
    {
        private ITestCaseCollection _testCaseCollection;
        private GraphDeltaProcessorHelper _graphDeltaProcessorHelper;

        public ServicePrincipalPreconditionValidationManager(ITestCaseCollection testCaseCollection, GraphDeltaProcessorHelper graphDeltaProcessorHelper = null)
        {
            _testCaseCollection = testCaseCollection;
            _graphDeltaProcessorHelper = graphDeltaProcessorHelper;
        }

        public ServicePrincipalWrapper ValidatePrecondition(ServicePrincipal servicePrincipal, TestCaseCollection.TestCase testCase)
        {
            string stateDefinitionClassName= _testCaseCollection.GetSpStateDefinition(testCase);

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);
            object[] args = { servicePrincipal , testCase};
            var instantiatedObject = Activator.CreateInstance(objectType, args) as ISpStateDefinition;
            return instantiatedObject.Validate();

        }
        internal ServicePrincipalWrapper GetNewServicePrincipalWrapper(ServicePrincipal servicePrincipal, TestCaseCollection.TestCase testCase)
        {

            string stateDefinitionClassName= _testCaseCollection.GetSpStateDefinition(testCase);


            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);
            object[] args = { servicePrincipal , testCase};
            var instantiatedObject = Activator.CreateInstance(objectType, args) as ISpStateDefinition;
            return instantiatedObject.GetNewServicePrincipalWrapper();
        }

        public bool DiscoverValidatePrecondition(IConfigurationRoot config, TestCaseCollection.TestCase testCase)
        {
            string stateDefinitionClassName= _testCaseCollection.GetSpStateDefinition(testCase);

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { config, testCase, _graphDeltaProcessorHelper};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IDiscoverSpStateDefinition;

            return instantiatedObject.Validate();
        }

        public ServicePrincipalWrapper UpdateValidatePrecondition(IConfigurationRoot config, TestCaseCollection.TestCase testCase)
        {
            string stateDefinitionClassName= _testCaseCollection.GetSpStateDefinition(testCase);

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Update.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { config, testCase};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IUpdateSpStateDefinition;

            return instantiatedObject.Validate();
        }
        public void Dispose()
        {
            // throw new NotImplementedException();
        }
    }
}
