using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    interface ISpResultValidator
    {
        public string SavedServicePrincipalAsString { get; }

        public ServicePrincipal NewServicePrincipal { get; }

        TestCase TestCaseID { get; }
        bool Validate();
    }
}
