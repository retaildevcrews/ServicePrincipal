using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    internal class CommonFunctions : IDisposable
    {

        internal enum TestCase { TC1, TC2, TC3, TC4, TC5, TC6, TC7, TC8, TC9 }

        private bool _initialized = false;

        internal CommonFunctions()
        {
            InitGraphHelper();
        }
        
        internal byte[] GetTestMessageContent(TestCase testCase)
        {
            ServicePrincipalWrapper spTest = GetServicePrincipalFor(testCase);

            var servicePrincipal = new ServicePrincipalModel()
            {
                Id = spTest.AADServicePrincipal.Id,  
                AppId = spTest.AADServicePrincipal.AppId,
                DisplayName = spTest.AADServicePrincipal.DisplayName,
                Notes = spTest.AADServicePrincipal.Notes,
                Owners = spTest.HasOwners ? spTest.AADUsers : null
            };

            var myMessage = new QueueMessage<ServicePrincipalModel>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = servicePrincipal,
                Attempt = 0
            };

            var payload = JsonConvert.SerializeObject(myMessage);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            return plainTextBytes;
            //System.Convert.ToBase64String(plainTextBytes)
        }

        private ServicePrincipalWrapper GetServicePrincipalFor(TestCase testCase)
        {
            ServicePrincipalWrapper result;
            switch (testCase)
            {
                case TestCase.TC1:
                    result = GetAADServicePrincipal(ConfigurationManager.AppSettings.Get(testCase.ToString()), testCase);
                    //result = GetAADServicePrincipal("sp-AzQueueTesting-1", testCase);
                    break;

                case TestCase.TC2:
                    result = GetAADServicePrincipal(ConfigurationManager.AppSettings.Get(testCase.ToString()), testCase);
                    break;

                case TestCase.TC3:
                    result = GetAADServicePrincipal(ConfigurationManager.AppSettings.Get(testCase.ToString()), testCase);
                    break;

                default:
                    throw new Exception($"Prerequisites are not setup for Test Case Id: {testCase}");
            }

            return result;
        }

        private ServicePrincipalWrapper GetAADServicePrincipal(string spDisplayName, TestCase testCase)
        {

            string servicePrincipalPrefix = ConfigurationManager.AppSettings.Get("servicePrincipalPrefix");
            //string servicePrincipalPrefix = "sp-AzQueueTesting";

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{servicePrincipalPrefix}").Result;

            ServicePrincipal spObject = servicePrincipalList.FirstOrDefault(x => x.DisplayName == spDisplayName);

            return ValidateServicePrincipalStateFor(spObject, testCase);
        }

        private ServicePrincipalWrapper ValidateServicePrincipalStateFor(ServicePrincipal spObject, TestCase testCase)
        {
            if (spObject == null)
            {
                throw new NullException(spObject);
            }


            ServicePrincipalWrapper result = new ServicePrincipalWrapper();

            switch (testCase)
            {
                case TestCase.TC1:
                    //-set owners 
                    //-populated Notes field with owners AAD emails

                    Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(spObject);
                    if (ownersList.Count > 0 && !string.IsNullOrEmpty(spObject.Notes))
                    {
                        foreach(var ownerName in ownersList.Values)
                        {
                            //var semicolonSeparatedOwnersEmail = string.Join(";", ownersList);
                            if (!spObject.Notes.Contains(ownerName))
                                throw new InvalidDataException($"Service Principal: [{spObject.DisplayName}] does not match Test Case [{testCase}] rules.");
                        }

                        result.SetAADServicePrincipal(spObject);
                        result.HasOwners = true;
                        result.AADUsers = ownersList.Keys.ToList();

                    }
                    else
                    {
                        return null;
                    }
                    break;
                case TestCase.TC2:
                    //-DO NOT set owners 
                    //-populated Notes field with owners AAD emails

                    break;
                case TestCase.TC3:
                    //-set owners 
                    //-populated Notes field with valid emails other that AAD emails
                    break;

                default:
                    throw new Exception($"Missing prerequisites for Test Case Id: {testCase}");
            }

            return result;
        }

        private void InitGraphHelper()
        {
            if (_initialized)
                return;
            string clientID = ConfigurationManager.AppSettings.Get("clientId"); 
            string clientSecret = ConfigurationManager.AppSettings.Get("clientSecret");
            string tenantId = ConfigurationManager.AppSettings.Get("tenantId");

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(clientID)
            .WithTenantId(tenantId)
            .WithClientSecret(clientSecret)
            .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Initialize Graph client
            GraphHelper.Initialize(authProvider);
            _initialized = true;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
