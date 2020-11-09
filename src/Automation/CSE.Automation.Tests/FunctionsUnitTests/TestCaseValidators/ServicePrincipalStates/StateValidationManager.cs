using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    internal class StateValidationManager : IDisposable
    {
        public ServicePrincipalWrapper Validate(ServicePrincipal servicePrincipal, TestCase testCase)
        {

            string stateDefinitionClassName= testCase.GetStateDefinition();
            string objectToInstantiate = $"CSE.Automation.FunctionsUnitTests.TestCaseStateValidators.ServicePrincipalStates.{stateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { servicePrincipal };

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IStateDefinition;

            return instantiatedObject.Validate();
        }
        public void Dispose()
        {         
           // throw new NotImplementedException();
        }
    }
}
