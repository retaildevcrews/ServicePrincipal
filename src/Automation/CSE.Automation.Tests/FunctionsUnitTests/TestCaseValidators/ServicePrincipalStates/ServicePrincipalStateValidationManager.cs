using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    internal class ServicePrincipalStateValidationManager : IDisposable
    {
        public ServicePrincipalWrapper ValidatePrecondition(ServicePrincipal servicePrincipal, TestCase testCase)
        {

            string stateDefinitionClassName= testCase.GetStateDefinition();
            string objectToInstantiate = $"CSE.Automation.FunctionsUnitTests.TestCaseStateValidators.ServicePrincipalStates.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { servicePrincipal , testCase};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IStateDefinition;

            return instantiatedObject.Validate();
        }
        public void Dispose()
        {         
           // throw new NotImplementedException();
        }
    }
}
