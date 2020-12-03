using CSE.Automation.Model;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string servicePrincipalNamePattern = $"{_spSettings.ServicePrincipalPrefix}-{_spSettings.ServicePrincipalBaseName}";
            var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalNamePattern).Result;

            int testCasesCount = 11;/// this numnber correct as off as of 10/23/2020
            int totalSPObjects = (testCasesCount * _spSettings.NumberOfSPObjectsToCreatePerTestCase);
            int numberOfServicePrincipalToCreate = totalSPObjects - servicePrincipalList.Count;

            if (servicePrincipalList.Count == 0)// none SP exist, create them all 
            {
                GraphHelper.CreateServicePrincipalAsync(servicePrincipalNamePattern, numberOfServicePrincipalToCreate);

                servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalNamePattern).Result;

                var applicationsList = GraphHelper.GetAllApplicationAsync(servicePrincipalNamePattern).Result;

                if (servicePrincipalList.Count != applicationsList.Count || totalSPObjects != servicePrincipalList.Count)
                {
                    throw new Exception($"Service Principal Count [{servicePrincipalList.Count}] mismatch Application Count [{applicationsList.Count}]");
                }

            }
            else if (numberOfServicePrincipalToCreate > 0)// if greater that zero we need to create more SPs , else we have more SPs than needed so we are OK
            {

                int maxSpId = GetMaxServicePrincipalId(servicePrincipalList);

                GraphHelper.CreateServicePrincipalAsync(servicePrincipalNamePattern, numberOfServicePrincipalToCreate, maxSpId + 1);

                servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalNamePattern).Result;

                //TODO: verify extra SP objects were created
            }


            return servicePrincipalList;
        }

        public List<User> GetOrCreateUsers()
        {
            string userNamePattern = $"{_spSettings.UserPrefix}-{_spSettings.UserBaseName}";

            var usersList = GraphHelper.GetAllUsers(userNamePattern).Result;

            int testCasesCount = 11;/// this numnber correct as of 10/23/2020
            int totalUserObjects = (testCasesCount * _spSettings.NumberOfUsersToCreatePerTestCase);
            int numberOfUsersToCreate = totalUserObjects - usersList.Count;


            if (usersList.Count == 0)// nono SP exist, create them all 
            {
                GraphHelper.CreateAADUsersAsync(userNamePattern, numberOfUsersToCreate);

                usersList = GraphHelper.GetAllUsers(userNamePattern).Result;

                if (totalUserObjects != usersList.Count)
                {
                    throw new Exception($"AAD Users Count [{usersList.Count}] mismatch the numbers of requested users to be created [{totalUserObjects}]");
                }

            }
            else if (numberOfUsersToCreate > 0)// if greater than zero we need to create more Users , else we have more Users than needed so we are OK
            {

                int maxSpId = GetMaxUserId(usersList);

                GraphHelper.CreateAADUsersAsync(userNamePattern, numberOfUsersToCreate, maxSpId + 1);

                usersList = GraphHelper.GetAllUsers(userNamePattern).Result;

                //TODO: verify extra User objects were created
            }


            return usersList;
        }

        internal void DeleteServicePrincipals()
        {
            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{_spSettings.ServicePrincipalPrefix}-{_spSettings.ServicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{_spSettings.ServicePrincipalPrefix}-{_spSettings.ServicePrincipalBaseName}").Result;


            GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList);

            GraphHelper.DeleteRegisteredApplicationsAsync(applicationsList);
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

        private int GetMaxUserId(IList<User> usersList)
        {
            List<int> sequenceList = new List<int>();
            foreach (var sp in usersList)
            {
                var index = int.Parse(sp.DisplayName.Split('-').ToList().Last());
                sequenceList.Add(index);
            }
            return sequenceList.Max();
        }

        private void DeleteServicePrincipal()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{_spSettings.ServicePrincipalPrefix}-{ _spSettings.ServicePrincipalBaseName}").Result;


            GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList);

            GraphHelper.DeleteRegisteredApplicationsAsync(applicationsList);

        }


        private void GetServicePrincipalAndRegisteredAppsCount()
        {

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{_spSettings.ServicePrincipalPrefix}-{_spSettings.ServicePrincipalBaseName}").Result;

            var applicationsList = GraphHelper.GetAllApplicationAsync($"{_spSettings.ServicePrincipalPrefix}-{_spSettings.ServicePrincipalBaseName}").Result;


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
