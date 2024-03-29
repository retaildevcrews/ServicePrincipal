using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphCrud
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;

        private static string deltaUserLinkValue;
        private static string deltaSPLinkValue;

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

            if (String.IsNullOrWhiteSpace(deltaUserLinkValue))
            {
                Console.WriteLine("No deltaLink found. Initializing...");
                userCollectionPage = await graphClient.Users.Delta().Request().GetAsync();    
            }
            else
            {
                userCollectionPage = new UserDeltaCollectionPage();
                userCollectionPage.InitializeNextPageRequest(graphClient, deltaUserLinkValue);
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
                deltaUserLinkValue = deltaLink.ToString();
            }

            return userList;

        }


        public static async Task<IEnumerable<ServicePrincipal>> GetServicePrincipalsDeltaAsync()
        {
            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage;
            

            var servicePrincipalList = new List<ServicePrincipal>();

            if (String.IsNullOrWhiteSpace(deltaSPLinkValue))
            {
                Console.WriteLine("No deltaLink found for Service Principal. Initializing...");
                servicePrincipalCollectionPage = await graphClient.ServicePrincipals.Delta().Request().GetAsync(); ;
            }
            else
            {
                servicePrincipalCollectionPage = new ServicePrincipalDeltaCollectionPage();
                servicePrincipalCollectionPage.InitializeNextPageRequest(graphClient, deltaSPLinkValue);
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync();
            }

            // Populate result
            servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync();
                servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            }


            if (servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object deltaLink))
            {
                deltaSPLinkValue = deltaLink.ToString();
            }

            return servicePrincipalList;

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

        public static async Task<User> GetUser(string userIdentitification)
        {
            var user = await graphClient.Users[userIdentitification].Request().GetAsync();
            Console.WriteLine($"Built request: {graphClient.Users[userIdentitification].RequestUrl}");

            return user;
        }

        public static async Task<IEnumerable<User>> GetUserOrUsersDeltaByFilter(string filter)
        {
            //This is just spike code so we'll only get the first page of the query from this request

            List<QueryOption> options = new List<QueryOption>
            {
                new QueryOption("$filter", filter)
            };

            try
            {
                //This works so long as I have the filter for id as 
                //string idQueryString = $"id eq '{ConfigurationManager.AppSettings.Get("userId")}'";
                //and not
                //string idQueryString = $"fields/id eq '{ConfigurationManager.AppSettings.Get("userId")}'";
                //Without .Delta() it will work either way

                IUserDeltaCollectionPage resultPage = await graphClient.Users.Delta().Request(options).Select("displayName, id, userPrincipalName").GetAsync();

                //alternatively pass filter to .Filter()
                //IUserDeltaCollectionPage resultPage = await graphClient.Users.Delta().Request().Filter(filter).Select("displayName, id").GetAsync();

                //Alternatively you can do graphClient.Users.Request().Filter(filterString).Select(...
                //var resultPage = await graphClient.Users.Request(options).Select("displayName, id, userPrincipalName").GetAsync();

                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Users: {ex.Message}");
                return null;
            }
        }

        public static async Task<ServicePrincipal> GetServicePrincipalAsync(string servicePrincipalId)
        {
            ServicePrincipal servicePrincipal = null;

            try
            {
                servicePrincipal = await graphClient.ServicePrincipals[servicePrincipalId].Request().GetAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Principal Id: {servicePrincipalId}  : {ex.Message}");
            }

            return servicePrincipal;
        }

        public static async void WriteServicePrincipalToCosmosAsync(string servicePrincipalId)
        {
            ServicePrincipal servicePrincipal = null;
            try
            {
                servicePrincipal = await GetServicePrincipalAsync(servicePrincipalId);
                var cosmosDB = await CosmosUtil.CreateAsync();
                await cosmosDB.AddServicePrincipalToContainerAsync(servicePrincipal);
                Console.WriteLine("Success writing service principal to Cosmos!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got the following exception while trying to write service principal: {servicePrincipal.Id} to cosmos");
                Console.WriteLine(ex.Message);
            }
        }
    }
}