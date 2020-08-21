using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Configuration;
using System.Collections.Specialized;


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

        static void addServicePrincipalNote()
        {
            Console.WriteLine("Enter Service Principal ID");
            string spId = Console.ReadLine();
            Console.WriteLine("Enter Note");
            string spNote = Console.ReadLine();

            GraphHelper.createUpdateServicePrincipalNote(spId, spNote);
        }
    }
}