using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
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
