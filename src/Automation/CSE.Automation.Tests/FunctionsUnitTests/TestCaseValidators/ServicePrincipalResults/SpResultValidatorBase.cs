using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{

    abstract class SpResultValidatorBase : ISpResultValidator
    {
        public string SavedServicePrincipalAsString { get; }

        public ServicePrincipal NewServicePrincipal { get; }

        public TestCase TestCaseID { get; }


        public SpResultValidatorBase(string savedServicePrincipalAsString, ServicePrincipal newServicePrincipal, TestCase testCase)
        {
            SavedServicePrincipalAsString = savedServicePrincipalAsString;
            NewServicePrincipal = newServicePrincipal;
            TestCaseID = testCase;
        }

        public abstract bool Validate();

       
    }
}
