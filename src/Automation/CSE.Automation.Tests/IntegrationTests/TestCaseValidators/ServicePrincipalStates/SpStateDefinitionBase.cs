using System.Collections.Generic;
using System.Linq;
using CSE.Automation.Model;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates
{
    abstract class SpStateDefinitionBase : ISpStateDefinition
    {
        public ServicePrincipal ServicePrincipalObject { get; }

        public ServicePrincipalModel SPModel { get; private set; }

        public TestCase TestCaseID { get; }

        public SpStateDefinitionBase(ServicePrincipal servicePrincipal, TestCase testCase)
        {
            ServicePrincipalObject = servicePrincipal;
            TestCaseID = testCase;
        }

        public abstract ServicePrincipalWrapper Validate();

        public ServicePrincipalWrapper GetNewServicePrincipalWrapper()
        {
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);
            return new ServicePrincipalWrapper(ServicePrincipalObject, ownersList.Keys.ToList(), true);
        }

    }
}
