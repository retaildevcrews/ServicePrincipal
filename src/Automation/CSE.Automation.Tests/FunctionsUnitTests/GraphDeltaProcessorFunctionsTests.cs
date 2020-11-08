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
using System.Collections.Generic;
using static CSE.Automation.Tests.FunctionsUnitTests.CommonFunctions;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private  readonly GraphDeltaProcessor _graphDeltaProcessor;
        private  readonly IServicePrincipalProcessor _processor;
        private  ServicePrincipalProcessorSettings _servicePrincipalProcessorSettings;

        private  ISecretClient _secretClient;
        private  IGraphHelper<ServicePrincipal> _graphHelper;
        private  IQueueServiceFactory _queueServiceFactory;
        private  IConfigService<ProcessorConfiguration> _configService;

        
        private  ObjectTrackingService _objectService;
        private  ObjectTrackingRepository _objectRespository;
        private  ObjectTrackingRepositorySettings _objectTrackingRepositorySettings;

        private  AuditService _auditService;
        private  AuditRepository _auditRespository;
        private  AuditRespositorySettings _auditRespositorySettings;

        private  ILogger<ServicePrincipalProcessor> _spProcessorLogger;
        private  ILogger<GraphDeltaProcessor> _graphLogger;
        private  ILogger<AuditService> _auditServiceLogger;
        private  ILogger<AuditRepository> _auditRepoLogger;
        private  ILogger<ObjectTrackingService> _objectTrackingServiceLogger;
        private  ILogger<ObjectTrackingRepository> _objectTrackingRepoLogger;

        private  IServiceProvider _serviceProvider;
        private  IConfigurationRoot _config;
        private  IModelValidatorFactory _modelValidatorFactory;

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

            _processor = new ServicePrincipalProcessor(_servicePrincipalProcessorSettings, _graphHelper, _queueServiceFactory,
                        _configService, _objectService, _auditService, _modelValidatorFactory, _spProcessorLogger);

            _graphDeltaProcessor = new GraphDeltaProcessor(_serviceProvider, _processor, _graphLogger, true);
        }

        private void CreateServices()
        {
            _auditRespository = new AuditRepository(_auditRespositorySettings, _auditRepoLogger);
            _auditService = new AuditService(_auditRespository, _auditServiceLogger);


            _objectRespository = new ObjectTrackingRepository(_objectTrackingRepositorySettings, _objectTrackingRepoLogger);
            _objectService = new ObjectTrackingService(_objectRespository, _auditService, _objectTrackingServiceLogger);
        }

        private void CreateSettings()
        {
            _auditRespositorySettings = new AuditRespositorySettings(_secretClient)
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
        }

        private void CreateLoggers()
        {
            _auditServiceLogger = CreateLogger<AuditService>();
            _auditRepoLogger = CreateLogger<AuditRepository>();
            _objectTrackingServiceLogger = CreateLogger<ObjectTrackingService>();
            _objectTrackingRepoLogger = CreateLogger<ObjectTrackingRepository>();
            _spProcessorLogger = CreateLogger<ServicePrincipalProcessor>();
            _graphLogger = CreateLogger<GraphDeltaProcessor>();
        }

        private void CreateMocks()
        {
            _queueServiceFactory = Substitute.For<IQueueServiceFactory>();
            _secretClient = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<IGraphHelper<ServicePrincipal>>();
            _serviceProvider = Substitute.For<IServiceProvider>();

            _modelValidatorFactory = Substitute.For<IModelValidatorFactory>();
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
                //.SetBasePath(appDirectory)
                .AddJsonFile("appconfig.json", true)
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            _config = configBuilder.Build();
        }
        

        [Fact]
        public void FunctionsTestScaffolding()
        {
            //TODO: This is basically scaffolding for the unit tests
            //for our functions
            Assert.True(true);
        }

        [Fact]
        public void FunctionEvaluateTestCase1()
        {
            using var commonFunctions = new CommonFunctions(_config);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(commonFunctions.GetTestMessageContent(TestCase.TC1));


            //this also works 
            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _graphLogger));
            thisTaks.Wait();

            // TODO: Add Verification for TestCase.TC1 for upon success  

            Assert.True(true);
        }


    }
}
