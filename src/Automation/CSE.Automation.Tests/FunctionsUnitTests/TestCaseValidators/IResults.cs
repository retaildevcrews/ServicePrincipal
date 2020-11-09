using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal interface IResults
    {
        void SaveState();

        bool Validate();
    }
}
