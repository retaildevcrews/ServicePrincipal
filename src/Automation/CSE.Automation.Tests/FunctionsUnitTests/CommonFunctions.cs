using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    internal class CommonFunctions : IDisposable
    {

        internal enum TestCase {
            [StateDefinition("StateDefinition1")]
            [Validator("Validator1")]
            TC1, 
            TC2, 
            TC3, 
            TC4, 
            TC5, 
            TC6, 
            TC7, 
            TC8, 
            TC9 
        }

        private bool _initialized = false;

        private readonly IConfigurationRoot _config;

        internal CommonFunctions(IConfigurationRoot config)
        {
            _config = config;
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
            
        }

        private ServicePrincipalWrapper GetServicePrincipalFor(TestCase testCase)
        {
            ServicePrincipalWrapper result;
            switch (testCase)
            {
                case TestCase.TC1:
                    result = GetAADServicePrincipal(_config[testCase.ToString()], testCase);
                    break;

                case TestCase.TC2:
                    result = GetAADServicePrincipal(_config[testCase.ToString()], testCase);
                    break;

                case TestCase.TC3:
                    result = GetAADServicePrincipal(_config[testCase.ToString()], testCase);
                    break;

                default:
                    throw new Exception($"Prerequisites are not setup for Test Case Id: {testCase}");
            }

            return result;
        }

        private ServicePrincipalWrapper GetAADServicePrincipal(string spDisplayName, TestCase testCase)
        {

            string servicePrincipalPrefix = _config["servicePrincipalPrefix"];
            

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

            using var stateValidationManager = new StateValidationManager();
            
            return stateValidationManager.Validate(spObject,  testCase);
            

        }

        private void InitGraphHelper()
        {
            if (_initialized)
                return;

            string clientID = _config["clientId"];
            string clientSecret = _config["clientSecret"];
            string tenantId = _config["tenantId"];

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
