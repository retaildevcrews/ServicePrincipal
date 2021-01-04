using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
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

        internal InputGeneratorBase(IConfigurationRoot config, GraphHelperSettings graphHelperSettings, ITestCaseCollection testCaseCollection, TestCaseCollection.TestCase testCaseId)
        {
            _config = config;
            TestCaseId = testCaseId;
            TestCaseCollection = testCaseCollection;
            InitGraphHelper(graphHelperSettings);
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

        private void InitGraphHelper(GraphHelperSettings graphHelperSettings)
        {
            if (_initialized)
                return;


            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(graphHelperSettings.GraphAppClientId)
            .WithTenantId(graphHelperSettings.GraphAppTenantId)
            .WithClientSecret(graphHelperSettings.GraphAppClientSecret)
            .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            TestEmailSettings emailSettings = new TestEmailSettings(_config);

            // Initialize Graph client
            GraphHelper.Initialize(authProvider, emailSettings);
            _initialized = true;
        }

        protected bool ValidateDiscoverServicePrincipalPrecondition(TestCaseCollection.TestCase testCase, GraphDeltaProcessorHelper graphDeltaProcessorHelper)
        {
            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager(TestCaseCollection, graphDeltaProcessorHelper);

            return stateValidationManager.DiscoverValidatePrecondition(_config,  testCase);

        }

        protected ServicePrincipalWrapper ValidateUpdateServicePrincipalPrecondition(TestCaseCollection.TestCase testCase)
        {
            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager(TestCaseCollection);

            return stateValidationManager.UpdateValidatePrecondition( _config, testCase);

        }

        protected void SetConfigId(string configId)
        {
            _configId = configId;
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

        public virtual ServicePrincipalModel GetServicePrincipalModel()
        {
            return _servicePrincipalWrapper.SPModel;
        }

        public virtual ServicePrincipal GetServicePrincipal(bool requery = false)
        {
            return GetServicePrincipalWrapper(requery).AADServicePrincipal;
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
