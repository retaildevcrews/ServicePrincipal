using CSE.Automation.Model;
using Microsoft.Graph;
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
       

        public List<ServicePrincipal> GetOrCreateServicePrincipals()
        {
            //check if we have enought SP objects created in AAD 
            //
            //Get SP objects or create the missing ones

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;

            int testCasesCount = 11;/// this numnber correct as off as of 10/23/2020

            int numberOfServicePrincipalToCreate = (testCasesCount * _spSettings.NumberOfSPObjectsToCreatePerTestCase) - servicePrincipalList.Count;

            
            if (servicePrincipalList.Count == 0 )// nono SP exist, create them all 
            {
                GraphHelper.CreateServicePrincipalAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}", numberOfServicePrincipalToCreate);

                servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;
            }
            else if (numberOfServicePrincipalToCreate  < 0)// if lees that zero we need to create more SPs , else if greater than zero means we have more SPs than needed so we are OK
            {
                //Delete all and recreate them , this should not really happen but it is 
                int maxSpId = GetMaxServicePrincipalId(servicePrincipalList);

                GraphHelper.CreateServicePrincipalAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}", numberOfServicePrincipalToCreate, maxSpId + 1);

                servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;
            }
            

            return servicePrincipalList;
        }

        private int GetMaxServicePrincipalId(IList<ServicePrincipal> servicePrincipalList)
        {
            List<int> sequenceList = new List<int>();
            foreach(var sp in servicePrincipalList)
            {
                var index = int.Parse(sp.DisplayName.Split('-').ToList().Last());
                sequenceList.Add(index);
            }
            return sequenceList.Max();
        }

        private void DeleteServicePrincipal()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipalsAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;


            GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList);

            GraphHelper.DeleteRegisteredApplicationsAsync(applicationsList);

        }


        private void GetServicePrincipalAndRegisteredAppsCount()
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
