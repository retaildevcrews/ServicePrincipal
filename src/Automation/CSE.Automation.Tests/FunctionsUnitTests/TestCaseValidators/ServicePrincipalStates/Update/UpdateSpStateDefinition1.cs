using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Update
{
    internal class UpdateSpStateDefinition1 : UpdateSpStateDefinitionBase, IUpdateSpStateDefinition
    {
        public UpdateSpStateDefinition1(IConfigurationRoot config, TestCase testCase) : base( config, testCase)
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

                GraphHelper.UpdateNotesFieldWithValidEmail(new List<ServicePrincipal>() { servicePrincipalObject });
                
                return new ServicePrincipalWrapper(servicePrincipalObject, ownersList.Values.ToList(), true);

            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to validate Service Principal [{ServicePrincipalName}] does not match Test Case [{TestCaseID}] rules.", ex);
            }

        }
    }
}
