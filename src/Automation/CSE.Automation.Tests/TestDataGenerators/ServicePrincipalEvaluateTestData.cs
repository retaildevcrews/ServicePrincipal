using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class ServicePrincipalEvaluateTestData
    {
        internal ServicePrincipalModel Target { get; set; }
        internal AuditCode[] ExpectedAuditCodes { get; set; }
        internal ServicePrincipalUpdateCommand ExpectedUpdateMessage { get; set; }
        internal TrackingModel[] ObjectServiceData { get; set; }

        public override string ToString()
        {
            return $"{Target.Id}, {Target.AppDisplayName}";
        }
    }
}
