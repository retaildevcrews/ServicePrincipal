using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;
using System.Runtime.CompilerServices;
using CSE.Automation.KeyVault;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.DataAccess;
using System.Reflection;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ConfigurationResults;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ActivityResults;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using System.IO;
using CSE.Automation.Model.Validators;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor _graphDeltaProcessor;
        private readonly IServicePrincipalProcessor _processor;
        private ServicePrincipalProcessorSettings _servicePrincipalProcessorSettings;

        private ISecretClient _secretClient;
        private IServicePrincipalGraphHelper _graphHelper;
        private IQueueServiceFactory _queueServiceFactory;

        private VersionMetadata _versionMetadata;


        private ObjectTrackingService _objectService;
        private ObjectTrackingRepository _objectRespository;
        private ObjectTrackingRepositorySettings _objectTrackingRepositorySettings;

        private AuditService _auditService;
        private AuditRepository _auditRespository;
        private AuditRepositorySettings _auditRespositorySettings;
        private AuditRepositoryTest _auditRespositoryTest;

        private ActivityService _activityService;
        private ActivityHistoryRepository _activityHistoryRespository;
        private ActivityHistoryRepositorySettings _activityRespositorySettings;

        private ConfigRepository _configRespository;
        private IConfigService<ProcessorConfiguration> _configService;
        private ConfigRespositorySettings _configRespositorySettings;

        private ILogger<ServicePrincipalProcessor> _spProcessorLogger;
        private ILogger<GraphDeltaProcessor> _graphLogger;

        private ILogger<AuditService> _auditServiceLogger;
        private ILogger<AuditRepository> _auditRepoLogger;
        private ILogger<ObjectTrackingService> _objectTrackingServiceLogger;
        private ILogger<ObjectTrackingRepository> _objectTrackingRepoLogger;
        private ILogger<ActivityService> _activityServiceLogger;
        private ILogger<ConfigRepository> _configRepoLogger;
        private ILogger<ConfigService> _configLogger;


        private ILogger<ServicePrincipalGraphHelperTest> _spGraphHelperLogger;
        private ILogger<UserGraphHelper> _userGraphLogger;
        private ILogger<AzureQueueService> _queueLogger;


        private IServiceProvider _serviceProvider;
        private ServiceCollection _builder;
        private IConfigurationRoot _config;
        private GraphHelperSettings _graphHelperSettings;

        private GraphDeltaProcessorWrapperFactory _graphDeltaProcessorWrapperFactory;


        public GraphDeltaProcessorFunctionsTests()
        {

            BuildConfiguration();

            //CreateMocks();
            CreateLoggers();
            CreateSettings();
            CreateServices();

            var modelValidatorFactory = _serviceProvider.GetService<IModelValidatorFactory>();

            _processor = new ServicePrincipalProcessor(_servicePrincipalProcessorSettings, _graphHelper, _queueServiceFactory, _configService,
                        _objectService, _auditService, _activityService, modelValidatorFactory, _spProcessorLogger);

            _graphDeltaProcessor = new GraphDeltaProcessor(_versionMetadata, _serviceProvider, _activityService, _processor, _graphLogger);

            _graphDeltaProcessorWrapperFactory = new GraphDeltaProcessorWrapperFactory(
                  _graphHelper, _queueServiceFactory, _configService, _objectService, _auditService, _activityService,
                  _spProcessorLogger, _versionMetadata, _serviceProvider, _processor, _graphLogger, _config, _secretClient, _configRespository);
        }

        private void CreateServices()
        {

            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = _config[Constants.KeyVaultName] };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = _config[Constants.AuthType].As<AuthenticationType>() };

            ICredentialService credentialService = new CredentialService(credServiceSettings);

            _secretClient = new SecretService(secretServiceSettings, credentialService);

            _graphHelperSettings = new GraphHelperSettings(_secretClient);

            var graphClient = new GraphClient(_graphHelperSettings);
            
            
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            _versionMetadata = new VersionMetadata(thisAssembly);

            _queueServiceFactory = new AzureQueueServiceFactory(_queueLogger);


            _auditRespository = new AuditRepository(_auditRespositorySettings, _auditRepoLogger);
            _auditService = new AuditService(_auditRespository, _auditServiceLogger);

            _auditRespositoryTest = new AuditRepositoryTest(_auditRespositorySettings, _auditRepoLogger);

            _objectRespository = new ObjectTrackingRepository(_objectTrackingRepositorySettings, _objectTrackingRepoLogger);
            _objectService = new ObjectTrackingService(_objectRespository, _auditService, _objectTrackingServiceLogger);

            _activityHistoryRespository = new ActivityHistoryRepository(_activityRespositorySettings, _auditRepoLogger);
            _activityService = new ActivityService(_activityHistoryRespository, _activityServiceLogger);

            _configRespository = new ConfigRepository(_configRespositorySettings, _configRepoLogger);
            _configService = new ConfigService(_configRespository, _configLogger);

            string displayNamePatternFilter = _config["displayNamePatternFilter"];

            _graphHelper = new ServicePrincipalGraphHelperTest(_graphHelperSettings, _auditService, graphClient ,displayNamePatternFilter,_spGraphHelperLogger);


            _builder = new ServiceCollection();

            _builder
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddSingleton<VersionMetadata>(_versionMetadata)

                .AddSingleton<ICredentialService>(x => new CredentialService(x.GetRequiredService<CredentialServiceSettings>()))
                .AddSingleton<ISecretClient>(_secretClient)

                .AddSingleton<IGraphServiceClient, GraphClient>(x => graphClient)

                .AddTransient<GraphHelperSettings>(x => _graphHelperSettings)
                .AddScoped<ILogger<ServicePrincipalGraphHelperTest>>(x => _spGraphHelperLogger)
                .AddScoped<ILogger<UserGraphHelper>>(x => _userGraphLogger)


                .AddScoped<IGraphServiceClient, GraphClient>(x => graphClient)

                .AddScoped<IAuditService>(x => _auditService)
               
                .AddScoped<IServicePrincipalGraphHelper, ServicePrincipalGraphHelperTest>(x => (ServicePrincipalGraphHelperTest)_graphHelper)
                .AddScoped<IGraphHelper<User>, UserGraphHelper>()
                .AddScoped<IModelValidator<GraphModel>, GraphModelValidator>()
                .AddScoped<IModelValidator<ServicePrincipalModel>, ServicePrincipalModelValidator>()
                .AddScoped<IModelValidator<AuditEntry>, AuditEntryValidator>()
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>();

            _serviceProvider = _builder.BuildServiceProvider();

        }

        private void CreateSettings()
        {

            _configRespositorySettings = new ConfigRespositorySettings(_secretClient)
            {
                Uri = _config[Constants.CosmosDBURLName],
                Key = _config[Constants.CosmosDBKeyName],
                DatabaseName = _config[Constants.CosmosDBDatabaseName],
                CollectionName = _config[Constants.CosmosDBConfigCollectionName]
            };

            _auditRespositorySettings = new AuditRepositorySettings(_secretClient)
            {
                Uri = _config[Constants.CosmosDBURLName],
                Key = _config[Constants.CosmosDBKeyName],
                DatabaseName = _config[Constants.CosmosDBDatabaseName],
                CollectionName = _config[Constants.CosmosDBAuditCollectionName]
            };

            _objectTrackingRepositorySettings = new ObjectTrackingRepositorySettings(_secretClient)
            {
                Uri = _config[Constants.CosmosDBURLName],
                Key = _config[Constants.CosmosDBKeyName],
                DatabaseName = _config[Constants.CosmosDBDatabaseName],
                CollectionName = _config[Constants.CosmosDBObjectTrackingCollectionName]
            };

            _servicePrincipalProcessorSettings = new ServicePrincipalProcessorSettings(_secretClient)
            {
                QueueConnectionString = _config[Constants.SPStorageConnectionString],
                EvaluateQueueName = _config[Constants.EvaluateQueueAppSetting.Trim('%')],
                UpdateQueueName = _config[Constants.UpdateQueueAppSetting.Trim('%')],
                DiscoverQueueName = _config[Constants.DiscoverQueueAppSetting.Trim('%')],
                ConfigurationId = _config["configId"].ToGuid(Guid.Parse("02a54ac9-441e-43f1-88ee-fde420db2559")),
                VisibilityDelayGapSeconds = _config["visibilityDelayGapSeconds"].ToInt(8),
                QueueRecordProcessThreshold = _config["queueRecordProcessThreshold"].ToInt(10),
                AADUpdateMode = _config["aadUpdateMode"].As<UpdateMode>(UpdateMode.Update)
                
            };


            _activityRespositorySettings = new ActivityHistoryRepositorySettings(_secretClient)
            {
                Uri = _config[Constants.CosmosDBURLName],
                Key = _config[Constants.CosmosDBKeyName],
                DatabaseName = _config[Constants.CosmosDBDatabaseName],
                CollectionName = _config[Constants.CosmosDBActivityHistoryCollectionName]

            };

        }

        private void CreateLoggers()
        {
            _auditServiceLogger = CreateLogger<AuditService>();
            _auditRepoLogger = CreateLogger<AuditRepository>();
            _objectTrackingServiceLogger = CreateLogger<ObjectTrackingService>();
            _objectTrackingRepoLogger = CreateLogger<ObjectTrackingRepository>();
            _spProcessorLogger = CreateLogger<ServicePrincipalProcessor>();
            _graphLogger = CreateLogger<GraphDeltaProcessor>();
            _activityServiceLogger = CreateLogger<ActivityService>();

            _spGraphHelperLogger = CreateLogger<ServicePrincipalGraphHelperTest>();
            _userGraphLogger = CreateLogger<UserGraphHelper>();
            _queueLogger = CreateLogger<AzureQueueService>();

            _configRepoLogger = CreateLogger<ConfigRepository>();
            _configLogger = CreateLogger<ConfigService>();
        }

        private ILogger<T> CreateLogger<T>()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .BuildServiceProvider();

            return serviceProvider.GetService<ILoggerFactory>().CreateLogger<T>();
        }


        private void BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", true)
                .AddEnvironmentVariables()
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            _config = configBuilder.Build();
        }

        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase1()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC1;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }


        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase2()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC2;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings,testCaseCollection, thisTestCase);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");


        }

        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase2_2()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC2_2;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase3() 
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC3;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");
        }

        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase3_2()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC3_2;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            //using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            //bool validObjectTracking =  objectTrackingValidationManager.Validate();

            //Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase4()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC4;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            //using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            //bool validObjectTracking =  objectTrackingValidationManager.Validate();

            //Assert.True(validObjectTracking, "Object Tracking Validation");

        }


        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase5()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC5;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            //using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            //bool validObjectTracking =  objectTrackingValidationManager.Validate();

            //Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact]
        [Trait("Category", "Integration Critical")]
        public void FunctionEvaluateTestCase6()
        {
            using var testCaseCollection = new EvaluateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC6;

            using var activityContext = _activityService.CreateContext($"Integration Test - EVALUATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new EvaluateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);

           
            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionDiscoverTestCase1()
        {
            using var testCaseCollection = new DiscoverTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC1;

            using var activityContext = _activityService.CreateContext($"Integration Test - DISCOVER Test Case [{thisTestCase}] ", withTracking: true);

            GraphDeltaProcessorWrapper graphDeltaProcessorWrapper  = _graphDeltaProcessorWrapperFactory.GetNewGraphDeltaProcessorWrapper();

            try
            {
                using var inputGenerator = new DiscoverInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase, graphDeltaProcessorWrapper.ConfigId);

                CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(DiscoveryMode.FullSeed, "HTTP", activityContext));

                ////Create Validators 
                using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext, false);

                using var activityValidationManager = new ActivityValidationManager(inputGenerator, _activityHistoryRespository, activityContext);

                using var configurationValidationManager = new ConfigurationValidationManager(inputGenerator, _configRespository, activityContext);


                Task thisTask = Task.Run (() => graphDeltaProcessorWrapper.GraphDeltaProcessorInstance.Discover(cloudQueueMessage, _graphLogger));
                thisTask.Wait();


                bool validServicePrincipal = servicePrincipalValidationManager.Validate();

                Assert.True(validServicePrincipal, "Service Principal Validation");

                bool validConfiguration =  configurationValidationManager.Validate();

                Assert.True(validConfiguration, "Configuration Validation");

                bool validActivity =  activityValidationManager.Validate();

                Assert.True(validActivity, "Activity Validation");
            }
            finally
            {
                _graphDeltaProcessorWrapperFactory.DeleteConfigItem(graphDeltaProcessorWrapper);
            }

        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionDiscoverTestCase1_2()
        {
            using var testCaseCollection = new DiscoverTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC1_2;

            using var activityContext = _activityService.CreateContext($"Integration Test - DISCOVER Test Case [{thisTestCase}] ", withTracking: true);

            GraphDeltaProcessorWrapper graphDeltaProcessorWrapper  = _graphDeltaProcessorWrapperFactory.GetNewGraphDeltaProcessorWrapper();

            using var graphDeltaProcessorHelper = new GraphDeltaProcessorHelper(graphDeltaProcessorWrapper.GraphDeltaProcessorInstance, _activityService, _graphLogger, _config, _configRespository, graphDeltaProcessorWrapper.ConfigId, _graphHelperSettings);

            try
            {
                using var inputGenerator = new DiscoverInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase, graphDeltaProcessorWrapper.ConfigId, graphDeltaProcessorHelper);


                CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(DiscoveryMode.Deltas, "HTTP", activityContext));


                ////Create Validators 
                using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext, false);

                using var activityValidationManager = new ActivityValidationManager(inputGenerator, _activityHistoryRespository, activityContext);

                using var configurationValidationManager = new ConfigurationValidationManager(inputGenerator, _configRespository, activityContext);


                Task thisTask = Task.Run (() => graphDeltaProcessorWrapper.GraphDeltaProcessorInstance.Discover(cloudQueueMessage, _graphLogger));
                thisTask.Wait();


                bool validServicePrincipal = servicePrincipalValidationManager.Validate();// Bug related to Discover Deltas

                Assert.True(validServicePrincipal, "Service Principal Validation");

                bool validConfiguration =  configurationValidationManager.Validate();

                Assert.True(validConfiguration, "Configuration Validation");

                bool validActivity =  activityValidationManager.Validate();

                Assert.True(validActivity, "Activity Validation");

            }
            finally
            {
                _graphDeltaProcessorWrapperFactory.DeleteConfigItem(graphDeltaProcessorWrapper);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionDiscoverTestCase2()
        {
            using var testCaseCollection = new DiscoverTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC2;

            using var activityContext = _activityService.CreateContext($"Integration Test - DISCOVER Test Case [{thisTestCase}] ", withTracking: true);

            GraphDeltaProcessorWrapper graphDeltaProcessorWrapper  = _graphDeltaProcessorWrapperFactory.GetNewGraphDeltaProcessorWrapper();

            try
            {
                using var inputGenerator = new DiscoverInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase, graphDeltaProcessorWrapper.ConfigId);

                CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(DiscoveryMode.FullSeed, "HTTP", activityContext));
                ////Create Validators 
                using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext, false);


                using var activityValidationManager = new ActivityValidationManager(inputGenerator, _activityHistoryRespository, activityContext);

                using var configurationValidationManager = new ConfigurationValidationManager(inputGenerator, _configRespository, activityContext);


                Task thisTask = Task.Run (() => graphDeltaProcessorWrapper.GraphDeltaProcessorInstance.Discover(cloudQueueMessage, _graphLogger));
                thisTask.Wait();


                bool validServicePrincipal = servicePrincipalValidationManager.Validate();

                Assert.True(validServicePrincipal, "Service Principal Validation");

                bool validConfiguration =  configurationValidationManager.Validate();

                Assert.True(validConfiguration, "Configuration Validation");

                bool validActivity =  activityValidationManager.Validate();

                Assert.True(validActivity, "Activity Validation");

            }
            finally
            {
                _graphDeltaProcessorWrapperFactory.DeleteConfigItem(graphDeltaProcessorWrapper);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionDiscoverTestCase3()
        {
            using var testCaseCollection = new DiscoverTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC3;

            GraphDeltaProcessorWrapper graphDeltaProcessorWrapper  = _graphDeltaProcessorWrapperFactory.GetNewGraphDeltaProcessorWrapper();

            using var graphDeltaProcessorHelper = new GraphDeltaProcessorHelper(graphDeltaProcessorWrapper.GraphDeltaProcessorInstance, _activityService, _graphLogger, _config, _configRespository, graphDeltaProcessorWrapper.ConfigId, _graphHelperSettings);

            try
            {
                using var inputGenerator = new DiscoverInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase, graphDeltaProcessorWrapper.ConfigId, graphDeltaProcessorHelper);


                using var activityContext = _activityService.CreateContext($"Integration Test - DISCOVER Test Case [{thisTestCase}] ", withTracking: true);

                CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(DiscoveryMode.Deltas, "HTTP", activityContext));

                ////Create Validators 
                using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext, false);

                using var activityValidationManager = new ActivityValidationManager(inputGenerator, _activityHistoryRespository, activityContext);

                using var configurationValidationManager = new ConfigurationValidationManager(inputGenerator, _configRespository, activityContext);

                Task thisTask = Task.Run (() => graphDeltaProcessorWrapper.GraphDeltaProcessorInstance.Discover(cloudQueueMessage, _graphLogger));
                thisTask.Wait();


                bool validServicePrincipal = servicePrincipalValidationManager.Validate();

                Assert.True(validServicePrincipal, "Service Principal Validation");

                bool validConfiguration =  configurationValidationManager.Validate();

                Assert.True(validConfiguration, "Configuration Validation");

                bool validActivity =  activityValidationManager.Validate();

                Assert.True(validActivity, "Activity Validation");

            }
            finally
            {
                _graphDeltaProcessorWrapperFactory.DeleteConfigItem(graphDeltaProcessorWrapper);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionUpdateTestCase1()
        {
            using var testCaseCollection = new UpdateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC1;

            using var activityContext = _activityService.CreateContext($"Integration Test - UPDATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new UpdateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.UpdateAAD(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionUpdateTestCase2()
        {

            using var testCaseCollection = new UpdateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC2;

            using var activityContext = _activityService.CreateContext($"Integration Test - UPDATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new UpdateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.UpdateAAD(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

  
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");


        }


        [Fact]
        [Trait("Category", "Integration")]
        public void FunctionUpdateTestCase3()
        {
            using var testCaseCollection = new UpdateTestCaseCollection();

            TestCase thisTestCase = testCaseCollection.TC3;

            using var activityContext = _activityService.CreateContext($"Integration Test - UPDATE Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new UpdateInputGenerator(_config, _graphHelperSettings, testCaseCollection, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent(activityContext));

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTask = Task.Run (() => _graphDeltaProcessor.UpdateAAD(cloudQueueMessage, _graphLogger));
            thisTask.Wait();

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");


        }
    }
}
