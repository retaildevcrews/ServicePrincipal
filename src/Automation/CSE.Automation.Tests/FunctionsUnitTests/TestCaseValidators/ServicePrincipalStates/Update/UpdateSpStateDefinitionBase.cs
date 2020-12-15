using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ConfigurationResults;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Update
{
    abstract class UpdateSpStateDefinitionBase : IUpdateSpStateDefinition
    {
        public TestCaseCollection.TestCase TestCaseID { get; }

        public  IConfigurationRoot Config { get; }

        public string DisplayNamePatternFilter { get;  }

        public string ServicePrincipalName { get; }
        
        public UpdateSpStateDefinitionBase(IConfigurationRoot config, TestCase testCase) 
        {
            TestCaseID = testCase;
            Config = config;
            ServicePrincipalName = Config[$"U_{TestCaseID}"];
            DisplayNamePatternFilter = Config["displayNamePatternFilter"];
        }

       
        abstract public ServicePrincipalWrapper Validate();
    }
}
