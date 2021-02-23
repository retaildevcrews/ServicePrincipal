using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class DiscoverServicePrincipalTestDataGenerator : IEnumerable<object[]>
    {
        private readonly ServicePrincipalDiscoverTestData[] data =
        {
            new ServicePrincipalDiscoverTestData
            {
                Target = new RequestDiscoveryCommand() { CorrelationId = Guid.NewGuid().ToString(), DiscoveryMode = DiscoveryMode.FullSeed, Source = "TEST" },
                InitialServicePrincipals = null,
                InitialObjectServiceData = null,
                ExpectedObjectServiceData = null,
                ExpectedAuditCodes = null,
            },
        };


        public IEnumerator<object[]> GetEnumerator()
        {
            return data.Select(item => new object[] { item }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
