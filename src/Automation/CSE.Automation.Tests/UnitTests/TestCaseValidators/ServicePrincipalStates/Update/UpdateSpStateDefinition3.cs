using System;
using System.Collections.Generic;
using System.Linq;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalStates.Update
{
    internal class UpdateSpStateDefinition3 : UpdateSpStateDefinitionBase, IUpdateSpStateDefinition
    {
        public UpdateSpStateDefinition3(IConfigurationRoot config, TestCase testCase) : base(config, testCase)
        {
        }
        public override ServicePrincipalWrapper Validate()
        {
            try
            {
                ServicePrincipal servicePrincipalObject = GraphHelper.GetServicePrincipal(ServicePrincipalName).Result;

                Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(servicePrincipalObject);
                if (ownersList.Count == 0)
                {
                    GraphHelper.AddOwner(servicePrincipalObject, Config["aadUserServicePrincipalPrefix"], 3);

                }

                GraphHelper.ClearNotesField(new List<ServicePrincipal>() { servicePrincipalObject });

          
                return new ServicePrincipalWrapper(servicePrincipalObject, ownersList.Values.ToList(), true);


            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to validate Service Principal [{ServicePrincipalName}] does not match Test Case [{TestCaseID}] rules.", ex);
            }
        }
    }
}
