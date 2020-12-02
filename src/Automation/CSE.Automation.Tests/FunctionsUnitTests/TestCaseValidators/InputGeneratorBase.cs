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

        protected readonly IConfigurationRoot _config;
        protected readonly ActivityContext _activityContext;
        private ServicePrincipalWrapper _servicePrincipalWrapper;

        public ITestCaseCollection TestCaseCollection { get; }

        public TestCase TestCaseId { get; }

        public string StorageConnectionString => _config["SPStorageConnectionString"]; 
        public string UpdateQueueName => _config["SPUpdateQueue"];

        public string AadUserServicePrincipalPrefix => _config["aadUserServicePrincipalPrefix"];

        internal InputGeneratorBase(IConfigurationRoot config, ActivityContext activityContext, ITestCaseCollection testCaseCollection, TestCaseCollection.TestCase testCaseId)
        {
            _config = config;
            _activityContext = activityContext;
            TestCaseId = testCaseId;
            TestCaseCollection = testCaseCollection;
            InitGraphHelper();
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

        protected bool ValidateDiscoverServicePrincipalPrecondition(TestCaseCollection.TestCase testCase)
        {
            using var stateValidationManager = new ServicePrincipalPreconditionValidationManager(TestCaseCollection);

            return stateValidationManager.DiscoverValidatePrecondition(_config["displayNamePatternFilter"],  testCase);

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
