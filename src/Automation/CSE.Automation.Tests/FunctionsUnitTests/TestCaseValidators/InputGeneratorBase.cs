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
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Xunit.Sdk;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    abstract internal class InputGeneratorBase : IInputGenerator, IDisposable
    {

        private bool _initialized = false;
        private string _configId;

        protected readonly IConfigurationRoot _config;
        
        private ServicePrincipalWrapper _servicePrincipalWrapper;

        public ITestCaseCollection TestCaseCollection { get; }

        public TestCase TestCaseId { get; }

        public string StorageConnectionString => _config["SPStorageConnectionString"]; 
        public string UpdateQueueName => _config["SPUpdateQueue"];

        public string EvaluateQueueName => _config["SPEvaluateQueue"];

        public string DiscoverQueueName => _config["SPDiscoverQueue"];

        public string DisplayNamePatternFilter => _config["displayNamePatternFilter"];

        public string AadUserServicePrincipalPrefix => _config["aadUserServicePrincipalPrefix"];

        public string ConfigId
        {
            get
            {
                return string.IsNullOrEmpty(_configId) ? _config["configId"] : _configId;
            }
        }
       

        internal InputGeneratorBase(IConfigurationRoot config,  ITestCaseCollection testCaseCollection, TestCaseCollection.TestCase testCaseId)
        {
            _config = config;
            TestCaseId = testCaseId;
            TestCaseCollection = testCaseCollection;
            InitGraphHelper();
        }

        protected void SetConfigId(string configId)
        {
            _configId = configId;
        }

        public ServicePrincipalModel GetServicePrincipalModel()
        {
            return _servicePrincipalWrapper.SPModel;
        }

        public ServicePrincipal GetServicePrincipal(bool requery = false)
        {
            return GetServicePrincipalWrapper(requery).AADServicePrincipal;
        }

        protected ServicePrincipalWrapper GetServicePrincipalWrapper(bool requery = false)
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

        private ServicePrincipalWrapper GetAADServicePrincipal(string spDisplayName, TestCaseCollection.TestCase testCase, bool getWrapperWithoutPreconditionValidation = false)
        {

            string servicePrincipalPrefix = _config["displayNamePatternFilter"];
            

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{servicePrincipalPrefix}").Result;

            ServicePrincipal spObject = servicePrincipalList.FirstOrDefault(x => x.DisplayName == spDisplayName);

            return ValidateServicePrincipalPreconditionStateFor(spObject, testCase, getWrapperWithoutPreconditionValidation);

        }

        private ServicePrincipalWrapper ValidateServicePrincipalPreconditionStateFor(ServicePrincipal spObject, TestCaseCollection.TestCase testCase,
                                        bool getWrapperWithoutPreconditionValidation = false)
        {
            if (spObject == null)
            {
                throw new NullException(spObject);
            }

            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager(TestCaseCollection);

            if (getWrapperWithoutPreconditionValidation)
                return stateValidationManager.GetNewServicePrincipalWrapper(spObject, testCase);
            else
                return stateValidationManager.ValidatePrecondition(spObject,  testCase);

        }

        protected bool ValidateDiscoverServicePrincipalPrecondition(TestCaseCollection.TestCase testCase, GraphDeltaProcessorHelper graphDeltaProcessorHelper)
        {
            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager(TestCaseCollection, graphDeltaProcessorHelper);

            return stateValidationManager.DiscoverValidatePrecondition(_config,  testCase);

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
