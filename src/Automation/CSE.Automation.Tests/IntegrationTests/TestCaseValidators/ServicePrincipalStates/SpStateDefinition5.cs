﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates
{
    internal class SpStateDefinition5 : SpStateDefinitionBase, ISpStateDefinition
    {
        public SpStateDefinition5(ServicePrincipal servicePrincipal, TestCase testCase) : base(servicePrincipal, testCase)
        {
        }
        public override ServicePrincipalWrapper Validate()
        {
            ServicePrincipalWrapper result = null;
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);

            if (ownersList.Count == 0 && !string.IsNullOrEmpty(ServicePrincipalObject.Notes))
            {
                // Emails in BusinessOwners must be Invalid
                List<string> invalidEmails = ServicePrincipalObject.Notes.Split(";").ToList();

                string tenantDomainName = GraphHelper.GetDomainName();
                foreach (var invalidEmail in invalidEmails)
                {
                    if (invalidEmail.EndsWith(tenantDomainName))
                    {
                        throw new InvalidDataException($"Service Principal: [{ServicePrincipalObject.DisplayName}] does not match Test Case [{TestCaseID}] rules.");
                    }
                }

                result = new ServicePrincipalWrapper(ServicePrincipalObject, ownersList.Values.ToList(), true);
            }

            if (result == null)
            {
                throw new InvalidDataException($"Service Principal: [{ServicePrincipalObject.DisplayName}] does not match Test Case [{TestCaseID}] rules.");
            }
  
            return result;

        }
    }
}
