using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingState
{
    interface IObjectStateDefinition
    {
        ObjectTrackingRepository Repository { get; }
        ActivityContext Context { get; }
        ServicePrincipalModel SPModel { get; }
        ServicePrincipal ServicePrincipalObject { get; }

        bool Validate();
    }
}
