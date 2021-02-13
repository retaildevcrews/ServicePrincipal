using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalStates
{
    internal class SpStateDefinition1 : SpStateDefinitionBase, ISpStateDefinition
    {
        public SpStateDefinition1(ServicePrincipal servicePrincipal, TestCase testCase) : base(servicePrincipal, testCase)
        {
        }
        public override ServicePrincipalWrapper Validate()
        {
            ServicePrincipalWrapper result = null;
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);
            if (ownersList.Count > 0 && !string.IsNullOrEmpty(ServicePrincipalObject.Notes))
            {
                foreach (var ownerName in ownersList.Values)
                {
                    if (!ServicePrincipalObject.Notes.Contains(ownerName))
                    {
                        throw new InvalidDataException($"Service Principal: [{ServicePrincipalObject.DisplayName}] does not match Test Case [{TestCaseID}] rules.");
                    }
                }

                result = new ServicePrincipalWrapper(ServicePrincipalObject, ownersList.Keys.ToList(),true);


            }

            if (result == null)
            {
                throw new InvalidDataException($"Service Principal: [{ServicePrincipalObject.DisplayName}] does not match Test Case [{TestCaseID}] rules.");
            }

            return result;
        }
    }
}
