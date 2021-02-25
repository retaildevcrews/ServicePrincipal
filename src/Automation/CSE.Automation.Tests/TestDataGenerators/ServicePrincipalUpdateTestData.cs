using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class ServicePrincipalUpdateTestData
    {
        internal ServicePrincipalUpdateCommand Target { get; set; }
        internal TrackingModel[] InitialObjectServiceData { get; set; }
        internal TrackingModel[] ExpectedObjectServiceData { get; set; }
        internal AuditCode[] ExpectedAuditCodes { get; set; }

        public override string ToString()
        {
            return $"{Target.Action}, {Target.Entity.Id}";
        }
    }
}
