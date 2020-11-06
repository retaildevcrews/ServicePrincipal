using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Model
{
    public class ServicePrincipalUpdateCommand
    {
        public string CorrelationId { get; set; }
        public string Id { get; set; }
        public (string Current, string Changed) Notes { get; set; }
        public string Message { get; set; }
    }
}
