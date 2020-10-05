using CSE.Automation.Model;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Graph
{
    public class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {
        public ServicePrincipalGraphHelper(GraphHelperSettings settings)
            : base(settings) {
        }

        public override async Task<(string, IEnumerable<ServicePrincipal>)> GetDeltaGraphObjects(string selectFields, ProcessorConfiguration config)
        {
            var servicePrincipalSeedList = new List<ServicePrincipal>();

            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage = await graphClient.ServicePrincipals
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
