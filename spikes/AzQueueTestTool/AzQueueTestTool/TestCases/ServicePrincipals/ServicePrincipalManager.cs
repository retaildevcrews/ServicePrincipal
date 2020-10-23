using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzQueueTestTool.TestCases.ServicePrincipals
{
    public class ServicePrincipalManager : IDisposable
    {

        private readonly ServicePrincipalSettings _spSettings;

        public ServicePrincipalManager(ServicePrincipalSettings spSettings)
        {
            _spSettings = spSettings;
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(_spSettings.ClientID)
            .WithTenantId(_spSettings.TenantId)
            .WithClientSecret(_spSettings.ClientSecret)
            .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Initialize Graph client
            GraphHelper.Initialize(authProvider);
        }
        public void CreateServicePrincipals()
        {
            if (!DoServicePrincipalExist(out List<string> missingServicePrincipalList))
                GenerateServicePrincipal();// need to pass SP list 
        }

        private bool DoServicePrincipalExist(out List<string> missingServicePrincipalList)
        {
            missingServicePrincipalList = new List<string>();
            //How about if only some of the SPs exist, we need to create the missing ones?
            return true;
        }

        public void DeleteServicePrincipal()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;


            GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList);

            GraphHelper.DeleteRegisteredApplicationsAsync(applicationsList);

        }

        public void GenerateServicePrincipal()
        {
            GraphHelper.CreateServicePrincipalAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}", _spSettings.NumberOfServicePrincipalToCreate);

            // Should we create Owners (AAD users) ??????
            // TODO Assign owners 

        }

        public void GetServicePrincipalAndRegisteredAppsCount()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;


            Console.WriteLine("Service Principal Objects Count: " + servicePrincipalList.Count());
            Console.WriteLine("Registered Apps Objects Count: " + applicationsList.Count());

            Console.ReadKey();
        }



        public  void GetCountForServicePrincipalsWithNotes()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsWithNotes($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;
            Console.WriteLine("Service Principal Objects with Notes : " + servicePrincipalList.Count());
            Console.ReadKey();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
