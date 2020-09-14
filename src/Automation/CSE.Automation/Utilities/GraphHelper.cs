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

        private static string deltaUserLinkValue;
        private static string deltaSPLinkValue;

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

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                // Only return the fields used by the application
                var resultPage = await graphClient.Users.Request()
                    .Select(e => new
                    {
                        e.DisplayName,
                    })
                    .GetAsync().ConfigureAwait(false);

                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Users: {ex.Message}");
                return null;
            }
        }



        public async Task<IEnumerable<ServicePrincipal>> GetAllServicePrincipalsAsync()
        {
            try
            {
                var servicePrincipals = await graphClient.ServicePrincipals.Request().GetAsync().ConfigureAwait(false);

                return servicePrincipals.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }


        public async Task<IEnumerable<User>> GetUsersDeltaAsync()
        {
            IUserDeltaCollectionPage userCollectionPage;

            var userList = new List<User>();

            if (String.IsNullOrWhiteSpace(deltaUserLinkValue))
            {
                //TODO: Move this Console.WriteLine to a log entry
                //Console.WriteLine("No deltaLink found. Initializing...");

                userCollectionPage = await graphClient.Users.Delta().Request().GetAsync().ConfigureAwait(false);
            }
            else
            {
                userCollectionPage = new UserDeltaCollectionPage();
                userCollectionPage.InitializeNextPageRequest(graphClient, deltaUserLinkValue);
                userCollectionPage = await userCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            // Populate result
            userList.AddRange(userCollectionPage.CurrentPage);

            while (userCollectionPage.NextPageRequest != null)
            {
                userCollectionPage = await userCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                userList.AddRange(userCollectionPage.CurrentPage);
            }


            if (userCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object deltaLink))
            {
                deltaUserLinkValue = deltaLink.ToString();
            }

            return userList;

        }


        public async Task<IEnumerable<ServicePrincipal>> GetServicePrincipalsDeltaAsync()
        {
            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage;


            var servicePrincipalList = new List<ServicePrincipal>();

            if (String.IsNullOrWhiteSpace(deltaSPLinkValue))
            {
                //TODO: Move to log entry
                //Console.WriteLine("No deltaLink found for Service Principal. Initializing...");
                servicePrincipalCollectionPage = await graphClient.ServicePrincipals.Delta().Request().GetAsync().ConfigureAwait(false) ;
            }
            else
            {
                servicePrincipalCollectionPage = new ServicePrincipalDeltaCollectionPage();
                servicePrincipalCollectionPage.InitializeNextPageRequest(graphClient, deltaSPLinkValue);
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            // Populate result
            servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            }


            if (servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object deltaLink))
            {
                deltaSPLinkValue = deltaLink.ToString();
            }

            return servicePrincipalList;

        }

        public async void createUpdateServicePrincipalNote(string servicePrincipalId, string servicePrincipalNote)
        {
            var servicePrincipal = new ServicePrincipal
            {
                Notes = servicePrincipalNote
            };

            try
            {
                await graphClient.ServicePrincipals[servicePrincipalId].Request().UpdateAsync(servicePrincipal).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Updating Notes for Service Principal Id: {servicePrincipalId}  : {ex.Message}");
                return;
            }

            //TODO: Move to log entry
            //Console.WriteLine("Service Principal Notes updated");

        }

    }
}