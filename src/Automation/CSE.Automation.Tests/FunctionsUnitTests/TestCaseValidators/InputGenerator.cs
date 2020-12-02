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
            [SpValidator("SpResultValidator2")]
            [ObjectValidator("ObjectResultValidator2")]
            [AuditValidator("AuditResultValidator2")]
            TC2,

            [SpStateDefinition("SpStateDefinition2_2")]
            [ObjectStateDefinition("ObjectStateDefinition2_2")]
            [SpValidator("SpResultValidator2_2")]
            [ObjectValidator("ObjectResultValidator2_2")]
            [AuditValidator("AuditResultValidator2_2")]
            TC2_2,

            [SpStateDefinition("SpStateDefinition3")]
            [ObjectStateDefinition("ObjectStateDefinition3")]
            [SpValidator("SpResultValidator3")]
            [ObjectValidator("ObjectResultValidator3")]
            [AuditValidator("AuditResultValidator3")]
            TC3,

            [SpStateDefinition("SpStateDefinition3_2")]
            [ObjectStateDefinition("ObjectStateDefinition3_2")]
            [SpValidator("SpResultValidator3_2")]
            [ObjectValidator("ObjectResultValidator3_2")]
            [AuditValidator("AuditResultValidator3_2")]
            TC3_2,
            
            [SpStateDefinition("SpStateDefinition4")]
            [ObjectStateDefinition("ObjectStateDefinition4")]
            [SpValidator("SpResultValidator4")]
            [ObjectValidator("ObjectResultValidator4")]
            [AuditValidator("AuditResultValidator4")]
            TC4,

            [SpStateDefinition("SpStateDefinition5")]
            [ObjectStateDefinition("ObjectStateDefinition5")]
            [SpValidator("SpResultValidator5")]
            [ObjectValidator("ObjectResultValidator5")]
            [AuditValidator("AuditResultValidator5")]
            TC5,

            [SpStateDefinition("SpStateDefinition6")]
            [ObjectStateDefinition("ObjectStateDefinition6")]
            [SpValidator("SpResultValidator6")]
            [ObjectValidator("ObjectResultValidator6")]
            [AuditValidator("AuditResultValidator6")]
            TC6, 
            TC7, 
            TC8, 
            TC9 
        }

      
        private bool _initialized = false;

        private readonly IConfigurationRoot _config;
        private readonly ActivityContext _activityContext;

        public TestCase TestCaseId { get; }
        public string StorageConnectionString => _config["SPStorageConnectionString"]; 
        public string UpdateQueueName => _config["SPUpdateQueue"];

        public string AadUserServicePrincipalPrefix => _config["aadUserServicePrincipalPrefix"];

        public string TC4AssignTheseOwnersWhenCreatingAMissingObjectTracking => _config["TC4_AssignTheseOwnersWhenCreatingAMissingObjectTracking"];

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
                _servicePrincipalWrapper = GetAADServicePrincipal(_config[TestCaseId.ToString()], TestCaseId, requery);

                if (_servicePrincipalWrapper == null)
                {
                    throw new Exception($"Unable to create ServicePrincipalWrapper for Test Case Id: {TestCaseId}");
                }
            }
            return _servicePrincipalWrapper;
           
        }

        private ServicePrincipalWrapper GetAADServicePrincipal(string spDisplayName, TestCase testCase, bool getWrapperWithoutPreconditionValidation = false)
        {

            string servicePrincipalPrefix = _config["displayNamePatternFilter"];
            

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{servicePrincipalPrefix}").Result;

            ServicePrincipal spObject = servicePrincipalList.FirstOrDefault(x => x.DisplayName == spDisplayName);

            return ValidateServicePrincipalPreconditionStateFor(spObject, testCase, getWrapperWithoutPreconditionValidation);

        }

        private ServicePrincipalWrapper ValidateServicePrincipalPreconditionStateFor(ServicePrincipal spObject, TestCase testCase,
                                        bool getWrapperWithoutPreconditionValidation = false)
        {
            if (spObject == null)
            {
                throw new NullException(spObject);
            }

            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager();
            
            if (getWrapperWithoutPreconditionValidation)
                return stateValidationManager.GetNewServicePrincipalWrapper(spObject, testCase);
            else
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
