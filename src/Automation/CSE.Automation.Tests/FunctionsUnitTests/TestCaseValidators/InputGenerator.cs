using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
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

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class InputGenerator : IDisposable
    {

        internal enum TestCase {
            [SpStateDefinition("SpStateDefinition1")]
            [ObjectStateDefinition("ObjectStateDefinition1")]
            [SpValidator("SpResultValidator1")]
            [ObjectValidator("ObjectResultValidator1")]
            [AuditValidator("AuditResultValidator1")]
            TC1,

            [SpStateDefinition("SpStateDefinition2")]
            [ObjectStateDefinition("ObjectStateDefinition2")]
            [SpValidator("SpResultValidator1")]// NOTE: TC2 uses same SpResultValidator as TC1
            [ObjectValidator("ObjectResultValidator2")]
            [AuditValidator("AuditResultValidator2")]
            TC2,

            [SpStateDefinition("SpStateDefinition2")]
            [ObjectStateDefinition("ObjectStateDefinition1")]// NOTE: TC2_2 uses same ObjectStateDefinition1 as TC1
            [SpValidator("SpResultValidator1")]// NOTE: TC2 uses same SpResultValidator as TC1
            [ObjectValidator("ObjectResultValidator2_2")]
            [AuditValidator("AuditResultValidator2")]
            TC2_2,

            [SpStateDefinition("SpStateDefinition3")]
            [SpValidator("SpResultValidator3")]
            [ObjectStateDefinition("ObjectStateDefinition3")]
            [ObjectValidator("ObjectResultValidator3")]
            [AuditValidator("AuditResultValidator3")]
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
        private readonly ActivityContext _activityContext;

        public TestCase TestCaseId { get; }

        private ServicePrincipalWrapper _servicePrincipalWrapper;


        internal InputGenerator(IConfigurationRoot config, ActivityContext activityContext,  TestCase testCase)
        {
            _config = config;
            _activityContext = activityContext;
            TestCaseId = testCase;

            InitGraphHelper();
        }
        
      
        internal byte[] GetTestMessageContent()
        {
            var spTest = GetServicePrincipalWrapper();

            var myMessage = new QueueMessage<EvaluateServicePrincipalCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new EvaluateServicePrincipalCommand
                {
                    CorrelationId = _activityContext.CorrelationId, 
                    Model = _servicePrincipalWrapper.SPModel,
                },
                Attempt = 0
            };

            var payload = JsonConvert.SerializeObject(myMessage);

            var plainTextBytes = Encoding.UTF8.GetBytes(payload);
            return plainTextBytes;
            
        }

        internal ServicePrincipalModel GetServicePrincipalModel()
        {
            return _servicePrincipalWrapper.SPModel;
        }

        internal ServicePrincipal GetServicePrincipal(bool requery = false)
        {
            return GetServicePrincipalWrapper(requery).AADServicePrincipal;
        }

        private ServicePrincipalWrapper GetServicePrincipalWrapper(bool requery = false)
        {
            if (_servicePrincipalWrapper == null || requery)
            {
                _servicePrincipalWrapper = GetAADServicePrincipal(_config[TestCaseId.ToString()], TestCaseId);

                if (_servicePrincipalWrapper == null)
                {
                    throw new Exception($"Unable to create ServicePrincipalWrapper for Test Case Id: {TestCaseId}");
                }
            }
            return _servicePrincipalWrapper;
           
        }

        private ServicePrincipalWrapper GetAADServicePrincipal(string spDisplayName, TestCase testCase)
        {

            string servicePrincipalPrefix = _config["servicePrincipalPrefix"];
            

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{servicePrincipalPrefix}").Result;

            ServicePrincipal spObject = servicePrincipalList.FirstOrDefault(x => x.DisplayName == spDisplayName);

            return ValidateServicePrincipalPreconditionStateFor(spObject, testCase);

        }

        private ServicePrincipalWrapper ValidateServicePrincipalPreconditionStateFor(ServicePrincipal spObject, TestCase testCase)
        {
            if (spObject == null)
            {
                throw new NullException(spObject);
            }

            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager();
            
            return stateValidationManager.ValidatePrecondition(spObject,  testCase);
            

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
