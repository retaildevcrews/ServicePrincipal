using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ObjectTrackingState
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
