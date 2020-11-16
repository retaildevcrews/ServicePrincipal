using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    internal class SpStateDefinition2_2 : SpStateDefinitionBase, ISpStateDefinition
    {
        public SpStateDefinition2_2(ServicePrincipal servicePrincipal, TestCase testCase) : base(servicePrincipal, testCase)
        {
        }
        public override ServicePrincipalWrapper Validate()
        {
            ServicePrincipalWrapper result = null;
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);

            if (ownersList.Count == 0 && !string.IsNullOrEmpty(ServicePrincipalObject.Notes))
            {
                if (GraphHelper.AreValidAADUsers(ServicePrincipalObject.Notes))
                {
                    result = new ServicePrincipalWrapper(ServicePrincipalObject, ownersList.Keys.ToList(), true);
                }
            }

            if (result == null)
            {
                throw new InvalidDataException($"Service Principal: [{ServicePrincipalObject.DisplayName}] does not match Test Case [{TestCaseID}] rules.");
            }
  
            return result;

        }
    }
}
