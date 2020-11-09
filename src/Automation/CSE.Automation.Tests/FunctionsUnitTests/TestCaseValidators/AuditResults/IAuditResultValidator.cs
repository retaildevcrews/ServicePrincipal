using System;
using System.Collections.Generic;
using System.Text;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    interface IAuditResultValidator
    {
        TestCase TestCaseID { get; }
        bool Validate();
    }
}
