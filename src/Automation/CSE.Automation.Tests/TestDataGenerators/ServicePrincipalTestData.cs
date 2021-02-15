using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class ServicePrincipalTestData
    {
        internal ServicePrincipalModel Model { get; set; }
        internal AuditCode[] ExpectedAuditCodes { get; set; }
        internal ServicePrincipalUpdateCommand ExpectedUpdateMessage { get; set; }
        internal TrackingModel<ServicePrincipalModel>[] ObjectServiceData { get; set; }

        public override string ToString()
        {
            return $"{Model.Id}, {Model.AppDisplayName}";
        }
    }
}
