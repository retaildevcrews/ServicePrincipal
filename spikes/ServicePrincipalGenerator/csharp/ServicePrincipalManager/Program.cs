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
        static int numberOfServicePrincipalToCreate = Int32.Parse(ConfigurationManager.AppSettings.Get("numberOfServicePrincipalToCreate"));
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

            //AddServicePrincipalNote(2, "Some Notes- Testing Mode");

            //GenerateServicePrincipal(numberOfServicePrincipalToCreate);

            DeleteServicePrincipal();

        }
        static void AddServicePrincipalNote(int count, string notes)
        {
            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}", count).Result;

            if (servicePrincipalList != null)
            {
                foreach (var spObject in servicePrincipalList)
                {
                    GraphHelper.CreateUpdateServicePrincipalNote(spObject.Id, notes);
                }
            }

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


        static void GenerateServicePrincipal(int count)
        {
            GraphHelper.CreateServicePrincipalAsync($"{servicePrincipalPrefix}-{ servicePrincipalBaseName}", count);
                       
        }


    }
}


