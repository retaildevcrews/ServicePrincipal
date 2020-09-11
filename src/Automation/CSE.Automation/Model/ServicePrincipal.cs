using System;

namespace CSE.Automation.Model
{
    class ServicePrincipal : GraphModel
    {
        public Guid AppId { get; set; }

        public string AppDisplayName { get; set; }

        public string DisplayName { get; set; }

        public string Notes { get; set; }
    }
}
