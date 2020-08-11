using Microsoft.Extensions.Configuration;
using System;

namespace GraphCrud
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Graph API Sample CRUD App \n");

            var appConfig = LoadAppSettings();

            if (appConfig == null)
            {
                Console.WriteLine("Missing or invalid appsettings.json...exiting");
                return;
            }

            var appId = appConfig["appId"];
            var scopesString = appConfig["scopes"];
            var scopes = scopesString.Split(';');

            // Initialize the auth provider with values from appsettings.json
            var authProvider = new DeviceCodeAuthProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = authProvider.GetAccessToken().Result;

            // Initialize Graph client
            GraphHelper.Initialize(authProvider);

            // Get signed in user
            var user = GraphHelper.GetMeAsync().Result;
            Console.WriteLine($"Welcome {user.DisplayName}!\n");

            int choice = -1;

            while (choice != 0)
            {
                Console.WriteLine("Please choose one of the following options:");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Display access token");
                Console.WriteLine("2. List All Users");
                Console.WriteLine("3. Add New User");
                Console.WriteLine("4. List Users (Delta)");

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
                        Console.WriteLine($"Access token: {accessToken}\n");
                        break;
                    case 2:
                        ListAllUsers();
                        break;
                    case 3:
                        createNewUser();
                        break;
                    case 4:
                        ListUsersDelta();
                        break;    
                    default:
                        Console.WriteLine("Invalid choice! Please try again.");
                        break;
                }
            }
        }

        static IConfigurationRoot LoadAppSettings()
        {
            var appConfig = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            // Check for required settings
            if (string.IsNullOrEmpty(appConfig["appId"]) ||
                string.IsNullOrEmpty(appConfig["scopes"]))
            {
                return null;
            }
            return appConfig;
        }

        static void ListAllUsers()
        {
            var users = GraphHelper.GetAllUsersAsync().Result;
        
            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.DisplayName}");
            }
        }

        static void ListUsersDelta()
        {
            var users = GraphHelper.GetUsersDeltaAsync().Result;
            
            foreach (var user in users)
            {
                Console.WriteLine(user.DisplayName + ":" + user.Id);
            }
        }


        static void createNewUser()
        {
            Console.WriteLine("Enter User First Name:");
            string firstName = Console.ReadLine();
            Console.WriteLine("Enter User Last Name:");
            string lastName = Console.ReadLine();
            GraphHelper.createUser(firstName, lastName);
        }
    }
}