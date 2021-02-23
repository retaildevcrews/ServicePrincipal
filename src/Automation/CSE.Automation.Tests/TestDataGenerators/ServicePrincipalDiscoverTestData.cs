using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class ServicePrincipalDiscoverTestData
    {
        internal RequestDiscoveryCommand Target { get; set; }
        internal TrackingModel[] InitialObjectServiceData { get; set; }
        internal TrackingModel[] ExpectedObjectServiceData { get; set; }
        internal AuditCode[] ExpectedAuditCodes { get; set; }
        internal ServicePrincipal[] InitialServicePrincipals { get; set; }

        public override string ToString()
        {
            return $"{Target.DiscoveryMode}, {Target.Source}";
        }
    }
}
