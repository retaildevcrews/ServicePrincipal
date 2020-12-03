using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    internal class ServicePrincipalPreconditionValidationManager : IDisposable
    {
        public ServicePrincipalWrapper ValidatePrecondition(ServicePrincipal servicePrincipal, TestCase testCase)
        {

            string stateDefinitionClassName= testCase.GetSpStateDefinition();
                                          
            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { servicePrincipal , testCase};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as ISpStateDefinition;

            return instantiatedObject.Validate();
        }
        internal ServicePrincipalWrapper GetNewServicePrincipalWrapper(ServicePrincipal servicePrincipal, TestCase testCase)
        {
            string stateDefinitionClassName= testCase.GetSpStateDefinition();

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { servicePrincipal , testCase};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as ISpStateDefinition;

            return instantiatedObject.GetNewServicePrincipalWrapper();
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }
    }
}
