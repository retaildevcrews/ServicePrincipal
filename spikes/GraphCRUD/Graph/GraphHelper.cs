using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphCrud
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;

        private static string deltaLinkValue;

        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        public static async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                var resultPage = await graphClient.Users.Request()
                // Only return the fields used by the application
                .Select(e => new
                {
                    e.DisplayName,
                })

                .GetAsync();
                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Users: {ex.Message}");
                return null;
            }
        }



        public static async Task<IEnumerable<ServicePrincipal>> GetAllServicePrincipalsAsync()
        {
            try
            {
                var servicePrincipals = await graphClient.ServicePrincipals
               .Request()
               .GetAsync();

                return servicePrincipals.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }

  
        public static async Task<IEnumerable<User>> GetUsersDeltaAsync()
        {
            IUserDeltaCollectionPage userCollectionPage;

            var userList = new List<User>();

            if (String.IsNullOrWhiteSpace(deltaLinkValue))
            {
                Console.WriteLine("No deltaLink found. Initializing...");
                userCollectionPage = await graphClient.Users.Delta().Request().GetAsync(); ;
            }
            else
            {
                userCollectionPage = new UserDeltaCollectionPage();
                userCollectionPage.InitializeNextPageRequest(graphClient, deltaLinkValue);
                userCollectionPage = await userCollectionPage.NextPageRequest.GetAsync();
            }

            // Populate result
            userList.AddRange(userCollectionPage.CurrentPage);

            while (userCollectionPage.NextPageRequest != null)
            {
                userCollectionPage = await userCollectionPage.NextPageRequest.GetAsync();
                userList.AddRange(userCollectionPage.CurrentPage);
            }


            if (userCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object deltaLink))
            {
                deltaLinkValue = deltaLink.ToString();
            }

            return userList;

        }

        public static async void createUpdateServicePrincipalNote(string servicePrincipalId, string servicePrincipalNote)
        {
            var servicePrincipal = new ServicePrincipal
            {
                Notes = servicePrincipalNote
            };

            try
            {
                await graphClient.ServicePrincipals[servicePrincipalId].Request().UpdateAsync(servicePrincipal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Updating Notes for Service Principal Id: {servicePrincipalId}  : {ex.Message}" );
                return;
            }

            Console.WriteLine("Service Principal Notes updated");

        }

    }
}