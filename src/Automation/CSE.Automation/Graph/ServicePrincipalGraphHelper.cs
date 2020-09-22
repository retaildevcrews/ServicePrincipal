using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Utilities
{
    public class ServicePrincipalGraphHelper : IGraphHelper<ServicePrincipal>
    {
        private static GraphServiceClient graphClient;
        private IConfidentialClientApplication confidentialClientApplication;

        public ServicePrincipalGraphHelper(string graphAppClientId, string graphAppTenantId, string graphAppClientSecret)
        {
            confidentialClientApplication = ConfidentialClientApplicationBuilder
           .Create(graphAppClientId)
           .WithTenantId(graphAppTenantId)
           .WithClientSecret(graphAppClientSecret)
           .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            graphClient = new GraphServiceClient(authProvider);
        }

        //TODO: Currently this function only does a full seed, need to use configuration to determine if doing delta vs seed
        public async Task<(string, IEnumerable<ServicePrincipal>)> GetDeltaGraphObjects(string selectFields, string deltaLink, Model.Configuration config)
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