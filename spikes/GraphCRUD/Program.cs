using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace GraphCrud
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Service Principal Graph API Spike\n");

            string clientId = ConfigurationManager.AppSettings.Get("clientId");
            string tenantId = ConfigurationManager.AppSettings.Get("tenantId");
            string clientSecret = ConfigurationManager.AppSettings.Get("clientSecret");

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithTenantId(tenantId)
            .WithClientSecret(clientSecret)
            .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Initialize Graph client
            GraphHelper.Initialize(authProvider);

            int choice = -1;

            while (choice != 0)
            {
                Console.WriteLine("Please choose one of the following options:");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. List All Users");
                Console.WriteLine("2. List Users (Delta)");
                Console.WriteLine("3. List All Service Principals");
                Console.WriteLine("4. Create/Update Service Principal Notes");
                Console.WriteLine("5. List Service Principal Deltas");
                Console.WriteLine("6. List fields for specific user");
                Console.WriteLine("7. List Filtered User Or Users Delta");
                Console.WriteLine("8. Write Service Principal To Cosmos");

                try
                {
                    choice = int.Parse(Console.ReadLine());
                }
                catch (System.FormatException)
                {
                    // Set to invalid value
                    choice = -1;
                }

                switch (choice)
                {
                    case 0:
                        Console.WriteLine("Goodbye...");
                        break;
                    case 1:
                        ListAllUsers();
                        break;
                    case 2:
                        ListUsersDelta();
                        break;
                    case 3:
                        ListServicePrincipals();
                        break;
                    case 4:
                        addServicePrincipalNote();
                        break;
                    case 5:
                        ListServicePrincipalDelta();
                        break;
                    case 6:
                        ListUserData();
                        break;
                    case 7:
                        ListFilteredUserOrUsersDelta();
                        break;
                    case 8:
                        WriteServicePrincipalToCosmos();
                        break;
                    default:
                        Console.WriteLine("Invalid choice! Please try again.");
                        break;
                }
            }
        }


        static void ListServicePrincipals()
        {
            var servicePrincipals = GraphHelper.GetAllServicePrincipalsAsync().Result;

            foreach (var sp in servicePrincipals)

            {
                Console.WriteLine($"{sp.DisplayName} - {sp.Id}");
            }

        }

        static void ListAllUsers()
        {
            var users = GraphHelper.GetAllUsersAsync().Result;

            foreach (var user in users)
            {
                Console.WriteLine($"{user.DisplayName} - {user.Id}");
            }
        }

        static void ListUsersDelta()
        {
            var usersDelta = GraphHelper.GetUsersDeltaAsync().Result;

            foreach (var user in usersDelta)
            {
                Console.WriteLine($"{user.DisplayName} - {user.Id}");
            }
        }

        static void ListServicePrincipalDelta()
        {
            var servicePrincipalDelta = GraphHelper.GetServicePrincipalsDeltaAsync().Result;

            foreach (var sp in servicePrincipalDelta)
            {
                Console.WriteLine($"Name: {sp.DisplayName} Id:{sp.Id} Notes: {sp.Notes}");
            }
        }

        static void addServicePrincipalNote()
        {
            Console.WriteLine("Enter Service Principal ID");
            string spId = Console.ReadLine();
            Console.WriteLine("Enter Note");
            string spNote = Console.ReadLine();

            GraphHelper.createUpdateServicePrincipalNote(spId, spNote);
        }

        static void ListFilteredUserOrUsersDelta()
        {
            //string idQueryString = $"fields/Id eq '{ConfigurationManager.AppSettings.Get("userId")}'";
            string idQueryString = $"id eq '{ConfigurationManager.AppSettings.Get("userId")}'";

            //Straight from the docs: https://docs.microsoft.com/en-us/graph/api/user-list?view=graph-rest-1.0&tabs=csharp, but it's not returning anything
            //string emailTenantQueryString = $"identities/any(c:c/issuerAssignedId eq '{ConfigurationManager.AppSettings.Get("principalName")}' and c/issuer eq '{ConfigurationManager.AppSettings.Get("tenantName")}')";

            //This is also not returning anything
            //string emailQueryString = $"Identities/any(id:id/IssuerAssignedId eq '{ConfigurationManager.AppSettings.Get("principalName")}')";

            //Works with non .Delta() request builder
            //string wildCardQueryString = $"startswith(givenName, '{ConfigurationManager.AppSettings.Get("userFirstName")}')";

            //Works with non .Delta() request builder
            //string wildCardBoolOpQueryString = $"startswith(givenName, '{ConfigurationManager.AppSettings.Get("userFirstName")}') and startswith(surname, '{ConfigurationManager.AppSettings.Get("userLastName")}')";

            IEnumerable<User> users = null;

            try
            {
                users = GraphHelper.GetUserOrUsersDeltaByFilter(idQueryString).Result;
                foreach (var user in users)
                {
                    Console.WriteLine($"{user.DisplayName} - {user.Id} - {user.UserPrincipalName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ListUserData()
        {
            //string uniqueUserIdentifier = ConfigurationManager.AppSettings.Get("principalName");
            string uniqueUserIdentifier = ConfigurationManager.AppSettings.Get("userId");

            try
            {
                var result = GraphHelper.GetUser(uniqueUserIdentifier);

                var user = result.Result;

                PropertyInfo[] propertyInfos = user.GetType().GetProperties();

                foreach (var property in propertyInfos)
                {
                    Console.WriteLine($"{property.Name}: {property.GetValue(user, null)}");
                }

                Console.WriteLine("identities: ");

                foreach (var identitiy in user.Identities)
                {
                    Console.WriteLine($"{identitiy.Issuer} {identitiy.IssuerAssignedId}");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void WriteServicePrincipalToCosmos()
        {
            try
            {
                GraphHelper.WriteServicePrincipalToCosmosAsync(ConfigurationManager.AppSettings.Get("ServicePrincipalId"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}