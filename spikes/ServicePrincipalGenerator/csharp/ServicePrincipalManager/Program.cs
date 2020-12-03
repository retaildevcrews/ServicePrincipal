using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ServicePrincipalNotesUpdater
{
    class Program
    {
        static string servicePrincipalPrefix = ConfigurationManager.AppSettings.Get("prefix");
        static string servicePrincipalBaseName = ConfigurationManager.AppSettings.Get("baseName");

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
            MainMenu();

            //GetCountForServicePrincipalsWithNotes(); 

            //GetServicePrincipalAndRegisteredAppsCount();

            //GenerateServicePrincipal();//>>>>>>>>> Creates SP objects

            //DeleteServicePrincipal();//>>>>>>>>> Deletes SP objects

        }

        private static void MainMenu()
        {
            int choice = -1;

            while (choice != 0)
            {
                Console.WriteLine("Please choose one of the following options:");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Update Service Principals Notes");


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
                        UpdateServicePrincipalNote();
                        break;

                    default:
                        Console.WriteLine("Invalid choice! Please try again.");
                        break;
                }
            }
        }

        static void UpdateServicePrincipalNote()
        {
            Console.WriteLine("Getting Service Principal Objects....");

            int count = int.Parse(ConfigurationManager.AppSettings.Get("numberOfServicePrincipalToUpdate"));
            string notes = ConfigurationManager.AppSettings.Get("Notes");

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}", count).Result;

            Console.WriteLine($"Updating Notes field for {servicePrincipalList.Count()} Service Principal Objects....");

            GraphHelper.UpdateServicePrincipalNote(notes, servicePrincipalList);
   

            Console.WriteLine("Service Principal Notes updated, press a key to continue");
            Console.ReadKey();
        }

        static void DeleteServicePrincipal()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}").Result;
            

            GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList);

            GraphHelper.DeleteRegisteredApplicationsAsync(applicationsList);

        }


        static void GenerateServicePrincipal()
        {
            int numberOfServicePrincipalToCreate = int.Parse(ConfigurationManager.AppSettings.Get("numberOfServicePrincipalToCreate"));
            GraphHelper.CreateServicePrincipalAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}", numberOfServicePrincipalToCreate);
                       
        }


        static void GetServicePrincipalAndRegisteredAppsCount()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}").Result;


            Console.WriteLine("Service Principal Objects Count: " + servicePrincipalList.Count());
            Console.WriteLine("Registered Apps Objects Count: " + applicationsList.Count());

            Console.ReadKey();
        }



        static void GetCountForServicePrincipalsWithNotes()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsWithNotes($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}").Result;
            Console.WriteLine("Service Principal Objects with Notes : " + servicePrincipalList.Count());
            Console.ReadKey();
        }

        

    }
}


