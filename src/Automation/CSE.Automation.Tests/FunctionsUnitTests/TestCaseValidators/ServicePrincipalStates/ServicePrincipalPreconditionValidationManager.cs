using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    internal class ServicePrincipalPreconditionValidationManager : IDisposable
    {
        private ITestCaseCollection _testCaseCollection;
    
        public ServicePrincipalPreconditionValidationManager(ITestCaseCollection testCaseCollection)
        {
            _testCaseCollection = testCaseCollection;
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

        public bool DiscoverValidatePrecondition(string displayNamePatternFilter, TestCaseCollection.TestCase testCase)
        {
            string stateDefinitionClassName= _testCaseCollection.GetSpStateDefinition(testCase);

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { displayNamePatternFilter, testCase};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IDiscoverSpStateDefinition;

            return instantiatedObject.Validate();
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }
    }
}
