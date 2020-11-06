using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    class ServicePrincipalWrapper
    {
        public ServicePrincipal AADServicePrincipal { get; private set; }
        public List<string> AADUsers { get; set; }
        public bool HasOwners { get; set; }
        public void SetAADServicePrincipal(ServicePrincipal spObject)
        {
            AADServicePrincipal = spObject;
        }
    }
}
