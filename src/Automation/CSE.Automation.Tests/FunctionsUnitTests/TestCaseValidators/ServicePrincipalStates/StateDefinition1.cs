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
    internal class StateDefinition1 : StateDefinitionBase, IStateDefinition
    {
        public StateDefinition1(ServicePrincipal servicePrincipal, TestCase testCase) : base(servicePrincipal, testCase)
        {
        }
        public override ServicePrincipalWrapper Validate()
        {
            ServicePrincipalWrapper result = new ServicePrincipalWrapper();
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);
            if (ownersList.Count > 0 && !string.IsNullOrEmpty(ServicePrincipalObject.Notes))
            {
                foreach (var ownerName in ownersList.Values)
                {
                    //var semicolonSeparatedOwnersEmail = string.Join(";", ownersList);
                    if (!ServicePrincipalObject.Notes.Contains(ownerName))
                        throw new InvalidDataException($"Service Principal: [{ServicePrincipalObject.DisplayName}] does not match Test Case [{TestCaseID}] rules.");
                }

                result.SetAADServicePrincipal(ServicePrincipalObject);
                result.HasOwners = true;
                result.AADUsers = ownersList.Keys.ToList();

                return result;

            }
            else
            {
                return null;
            }

        }
    }
}
