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
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;
using System.Runtime.CompilerServices;
using CSE.Automation.Validators;
using CSE.Automation.KeyVault;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.DataAccess;
using System.Reflection;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using System.IO;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor _graphDeltaProcessor;
        private readonly IServicePrincipalProcessor _processor;
        private ServicePrincipalProcessorSettings _servicePrincipalProcessorSettings;

        private ISecretClient _secretClient;
        private IGraphHelper<ServicePrincipal> _graphHelper;
        private IQueueServiceFactory _queueServiceFactory;
        private IConfigService<ProcessorConfiguration> _configService;
        private VersionMetadata _versionMetadata;


        private ObjectTrackingService _objectService;
        private ObjectTrackingRepository _objectRespository;
        private ObjectTrackingRepositorySettings _objectTrackingRepositorySettings;

        private AuditService _auditService;
        private AuditRepository _auditRespository;
        private AuditRepositorySettings _auditRespositorySettings;
        private AuditRepositoryTest _auditRespositoryTest;

        private ActivityService _activityService;
        private ActivityHistoryRepository _auditHistoryRespository;
        private ActivityHistoryRepositorySettings _activityRespositorySettings;

        private ILogger<ServicePrincipalProcessor> _spProcessorLogger;
        private ILogger<GraphDeltaProcessor> _graphLogger;
        private ILogger<AuditService> _auditServiceLogger;
        private ILogger<AuditRepository> _auditRepoLogger;
        private ILogger<ObjectTrackingService> _objectTrackingServiceLogger;
        private ILogger<ObjectTrackingRepository> _objectTrackingRepoLogger;
        private ILogger<ActivityService> _activityServiceLogger;

        private ILogger<ServicePrincipalGraphHelperTest> _spGraphHelperLogger;
        private ILogger<UserGraphHelper> _userGraphLogger;
        private ILogger<AzureQueueService> _queueLogger;



        private IServiceProvider _serviceProvider;
        private ServiceCollection _builder;
        private IConfigurationRoot _config;


        public GraphDeltaProcessorFunctionsTests()
        {
            // TODO: Need to add an interfaces for these so we can mock them or come up with another way to instantiate 
            // for testing. As it is right now the substitution won't work because the
            // constructors will actually get called and GraphHelperBase will try to create a graph client.


            BuildConfiguration();

            CreateMocks();
            CreateLoggers();
            CreateSettings();
            CreateServices();

            var modelValidatorFactory = _serviceProvider.GetService<IModelValidatorFactory>();

            _processor = new ServicePrincipalProcessor(_servicePrincipalProcessorSettings, _graphHelper, _queueServiceFactory, _configService,
                        _objectService, _auditService, _activityService, modelValidatorFactory, _spProcessorLogger);

            _graphDeltaProcessor = new GraphDeltaProcessor(_versionMetadata, _serviceProvider, _activityService, _processor, _graphLogger);
        }

        private void CreateServices()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            _versionMetadata = new VersionMetadata(thisAssembly);

            _queueServiceFactory = new AzureQueueServiceFactory(_queueLogger);

            _auditRespository = new AuditRepository(_auditRespositorySettings, _auditRepoLogger);
            _auditService = new AuditService(_auditRespository, _auditServiceLogger);

            _auditRespositoryTest = new AuditRepositoryTest(_auditRespositorySettings, _auditRepoLogger);

            _objectRespository = new ObjectTrackingRepository(_objectTrackingRepositorySettings, _objectTrackingRepoLogger);
            _objectService = new ObjectTrackingService(_objectRespository, _auditService, _objectTrackingServiceLogger);

            _auditHistoryRespository = new ActivityHistoryRepository(_activityRespositorySettings, _auditRepoLogger);
            _activityService = new ActivityService(_auditHistoryRespository, _activityServiceLogger);


            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = _config[Constants.KeyVaultName] };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = _config[Constants.AuthType].As<AuthenticationType>() };


            string displayNamePatternFilter = _config["displayNamePatternFilter"];

            var credentialService = new CredentialService(credServiceSettings);
            var secretClient = new SecretService(secretServiceSettings, credentialService);
            var graphHelperSettings = new GraphHelperSettings(secretClient);

            var graphClient = new GraphClient(graphHelperSettings);

            var servicePrincipalGraphHelperTest = new ServicePrincipalGraphHelperTest(graphHelperSettings, _auditService, graphClient ,displayNamePatternFilter,_spGraphHelperLogger);


            _builder = new ServiceCollection();

            _builder
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddSingleton<VersionMetadata>(_versionMetadata)

                .AddSingleton<ICredentialService>(x => new CredentialService(x.GetRequiredService<CredentialServiceSettings>()))
                .AddSingleton<ISecretClient>(secretClient)

                .AddSingleton<IGraphServiceClient, GraphClient>(x => graphClient)

                .AddTransient<GraphHelperSettings>(x => graphHelperSettings)
                .AddScoped<ILogger<ServicePrincipalGraphHelperTest>>(x => _spGraphHelperLogger)
                .AddScoped<ILogger<UserGraphHelper>>(x => _userGraphLogger)

                .AddScoped<IAuditService>(x => _auditService)

                .AddScoped<IGraphHelper<ServicePrincipal>, ServicePrincipalGraphHelperTest>(x => servicePrincipalGraphHelperTest)
                .AddScoped<IGraphHelper<User>, UserGraphHelper>()
                .AddScoped<IModelValidator<GraphModel>, GraphModelValidator>()
                .AddScoped<IModelValidator<ServicePrincipalModel>, ServicePrincipalModelValidator>()
                .AddScoped<IModelValidator<AuditEntry>, AuditEntryValidator>()
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>();

            _serviceProvider = _builder.BuildServiceProvider();



        }

        private void CreateSettings()
        {
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

            _servicePrincipalProcessorSettings = new ServicePrincipalProcessorSettings(_secretClient)// need this to setup queues
            {
                QueueConnectionString = _config[Constants.SPStorageConnectionString],
                EvaluateQueueName = _config[Constants.EvaluateQueueAppSetting.Trim('%')],
                UpdateQueueName = _config[Constants.UpdateQueueAppSetting.Trim('%')],
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
        }

        private void CreateMocks()
        {

            _secretClient = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<IGraphHelper<ServicePrincipal>>();
            _configService = Substitute.For<IConfigService<ProcessorConfiguration>>();
            
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
                .AddJsonFile("appconfig.json", false)
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            string devConfigPath = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "\\appconfig.Development.json");
            if (System.IO.File.Exists(devConfigPath))
            {
                configBuilder.AddJsonFile("appconfig.Development.json", true);
            }

            _config = configBuilder.Build();
        }

        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase1()
        {
            TestCase thisTestCase = TestCase.TC1;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }


        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase2()
        {

            TestCase thisTestCase = TestCase.TC2;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");


        }

        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase2_2()
        {
            TestCase thisTestCase = TestCase.TC2_2;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator
            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase3() 
        {
            TestCase thisTestCase = TestCase.TC3;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

         }

        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase3_2()
        {
            TestCase thisTestCase = TestCase.TC3_2;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase4()
        {
            TestCase thisTestCase = TestCase.TC4;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }


        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase5()
        {
            TestCase thisTestCase = TestCase.TC5;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }

        [Fact(Skip = "Needs Updating")]
        [Trait("Category","Integration")]
        public void FunctionEvaluateTestCase6()
        {
            TestCase thisTestCase = TestCase.TC6;

            using var activityContext = _activityService.CreateContext($"Unit Test - Test Case [{thisTestCase}] ", withTracking: true);

            using var inputGenerator = new InputGenerator(_config, activityContext, thisTestCase);


            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(inputGenerator.GetTestMessageContent());

            //Create Validators 
            using var servicePrincipalValidationManager = new ServicePrincipalValidationManager(inputGenerator, activityContext);

            using var objectTrackingValidationManager = new ObjectTrackingValidationManager(inputGenerator, _objectRespository, activityContext);

            using var auditValidationManager = new AuditValidationManager(inputGenerator, _auditRespositoryTest, activityContext);


            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            //Validate Outcome and state after execution for Service Principal, Audit and ObjectTracking objects based on TestCase injected thru InputGenerator

            bool validServicePrincipal = servicePrincipalValidationManager.Validate();

            Assert.True(validServicePrincipal, "Service Principal Validation");

            bool validAudit =  auditValidationManager.Validate();

            Assert.True(validAudit, "Audit Validation");

            bool validObjectTracking =  objectTrackingValidationManager.Validate();

            Assert.True(validObjectTracking, "Object Tracking Validation");

        }
    }
}
