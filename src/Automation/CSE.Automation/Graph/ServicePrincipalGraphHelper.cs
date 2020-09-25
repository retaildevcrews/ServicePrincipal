using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Graph
{
    public class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {
        public ServicePrincipalGraphHelper(string graphAppClientId, string graphAppTenantId, string graphAppClientSecret)
            : base(graphAppClientId, graphAppTenantId, graphAppClientSecret) {
        }

        public override async Task<(string, IEnumerable<ServicePrincipal>)> GetDeltaGraphObjects(string selectFields, Model.ProcessorConfiguration config)
        {
            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage;

            var servicePrincipalSeedList = new List<ServicePrincipal>();

            servicePrincipalCollectionPage = await graphClient.ServicePrincipals
                .Delta()
                .Request()
                .Select(selectFields)
                .GetAsync()
                .ConfigureAwait(true);

            servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            }

            servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);

            return (updatedDeltaLink.ToString(), servicePrincipalSeedList);
        }
    }
}