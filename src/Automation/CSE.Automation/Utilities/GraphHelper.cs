using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Utilities
{
    public class GraphHelper : IGraphHelper
    {
        private static GraphServiceClient graphClient;
        private IConfidentialClientApplication confidentialClientApplication;

        public GraphHelper(string graphAppClientId, string graphAppTenantId, string graphAppClientSecret)
        {
            confidentialClientApplication = ConfidentialClientApplicationBuilder
           .Create(graphAppClientId)
           .WithTenantId(graphAppTenantId)
           .WithClientSecret(graphAppClientSecret)
           .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            graphClient = new GraphServiceClient(authProvider);
        }

        public async Task<IEnumerable<ServicePrincipal>> SeedServicePrincipalDeltaAsync(string selectSPFields)
        {
            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage;

            var servicePrincipalSeedList = new List<ServicePrincipal>();

            servicePrincipalCollectionPage = await graphClient.ServicePrincipals
                .Delta()
                .Request()
                .Select(selectSPFields)
                .GetAsync()
                .ConfigureAwait(true);

            servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            }

            if (servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object deltaLink))
            {
                //TODO save this delta link in cosmosDB when we do a seed
               // Console.WriteLine("Seed Delta Link:" + deltaLink.ToString());
            }

            return servicePrincipalSeedList;
        }

        public async Task<IEnumerable<ServicePrincipal>> GetServicePrincipalsByDeltaAsync(string deltaLink)
        {
            if (String.IsNullOrWhiteSpace(deltaLink))
            {
                //TODO log this error
                throw new Exception("deltaLink value cannot be null or empty");
            }

            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage;

            var servicePrincipalList = new List<ServicePrincipal>();

            servicePrincipalCollectionPage = new ServicePrincipalDeltaCollectionPage();
            servicePrincipalCollectionPage.InitializeNextPageRequest(graphClient, deltaLink);
            servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);

            servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            }

            if (servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink))
            {
                //TODO save this delta link in cosmosDB when we do a seed
                //Console.WriteLine("Updated Delta Link:" + updatedDeltaLink.ToString());
            }

            return servicePrincipalList;

        }
    }
}