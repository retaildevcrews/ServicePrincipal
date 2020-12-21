using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.TestsPrep.TestCases.ServicePrincipals
{
    interface IEmailSettings
    {
        string TestEmailBase { get; }

        bool IncludeRandomStringToTestEmail { get; }

        int TestEmailCount { get; }

        List<string> TestEmailDomainNames { get; }
    }
}
