using System.Collections.Generic;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.Helpers
{
    internal class ServicePrincipalGraphHelperTest : ServicePrincipalGraphHelper
    {
        private string _displayNamePatternFilter;

        public ServicePrincipalGraphHelperTest(GraphHelperSettings settings, IAuditService auditService, IGraphServiceClient graphClient, 
            string displayNamePatternFilter, ILogger<ServicePrincipalGraphHelper> logger) : base(settings, auditService, graphClient, logger)
        {
            _displayNamePatternFilter = displayNamePatternFilter;
        }

        protected override IServicePrincipalDeltaRequest GetGraphSeedRequest()
        {
            return GraphClient
                .ServicePrincipals
                .Delta()
                .Request()
                .Filter(GetFilterString(_displayNamePatternFilter));
        }

        private string GetFilterString(string displayNamePatternFilter)
        {
            List<ServicePrincipal> servicePrincipalList = new List<ServicePrincipal>();

            var servicePrincipalsPage = GraphClient.ServicePrincipals
                .Request()
                .Filter($"startswith(displayName,'{displayNamePatternFilter}')")
                .GetAsync().Result;

            servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);

            // NOTE: The number of ids you can specify is limited by the maximum URL length
            // Successfully tested a request like this Delta().Request().Filter("filter string for up to 200 SPs") with 200 SP IDs so 100 should not be a problem.
            while (servicePrincipalsPage.NextPageRequest != null && servicePrincipalList.Count < 100)
            {
                servicePrincipalsPage = servicePrincipalsPage.NextPageRequest.GetAsync().Result;
                servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);
            }

            string filterTemplate = string.Empty;

            foreach (var spObject in servicePrincipalList)
            {
                if (string.IsNullOrEmpty(filterTemplate))
                {
                    filterTemplate = $"id eq '{spObject.Id}'";
                }
                else
                {
                    filterTemplate += $" or id eq '{spObject.Id}'";
                }
            }

            DeleteServicePrincial();

            return filterTemplate;
        }

        private void DeleteServicePrincial()
        {
            string servicePrincipalToDelete = $"{_displayNamePatternFilter}{TestCaseCollection.TestRemovedAttributeSuffix}";

            using ServicePrincipalHelper servicePrincipalHelper = new ServicePrincipalHelper();
            servicePrincipalHelper.DeleteServicePrincipal(servicePrincipalToDelete);
        }
    }
}
