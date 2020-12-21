using System;
using System.Collections.Generic;
using System.IO;
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
            if (string.IsNullOrEmpty(ServicePrincipalName))
            {
                throw new InvalidDataException($"Configuration setting 'U_{TestCaseID}' is null or empty");
            }

            DisplayNamePatternFilter = Config["displayNamePatternFilter"];

            if (string.IsNullOrEmpty(DisplayNamePatternFilter))
            {
                throw new InvalidDataException("Configuration setting 'displayNamePatternFilter' is null or empty");
            }
        }

       
        abstract public ServicePrincipalWrapper Validate();
    }
}
