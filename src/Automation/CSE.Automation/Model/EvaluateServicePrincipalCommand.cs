using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Model
{
    internal class EvaluateServicePrincipalCommand
    {
        public string CorrelationId { get; set; }
        public ServicePrincipalModel Model { get; set; }
    }
}
