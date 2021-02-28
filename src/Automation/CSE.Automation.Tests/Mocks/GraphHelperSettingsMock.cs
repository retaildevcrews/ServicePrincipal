using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Graph;

namespace CSE.Automation.Tests.Mocks
{
    internal class GraphHelperSettingsMock : IGraphHelperSettings
    {
        public string GraphAppClientId { get; }
        public string GraphAppTenantId { get; }
        public string GraphAppClientSecret { get; }
        public void Validate()
        {
            
        }
    }
}
