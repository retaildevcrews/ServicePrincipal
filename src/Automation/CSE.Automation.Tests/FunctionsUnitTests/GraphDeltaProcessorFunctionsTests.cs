using CSE.Automation.DataAccess;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static CSE.Automation.Tests.FunctionsUnitTests.CommonFunctions;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor _graphDeltaProcessor;

        private readonly ServicePrincipalProcessorSettings _servicePrincipalProcessorSettings;

        private readonly ISecretClient _secretClient;
        private readonly IGraphHelper<ServicePrincipal> _graphHelper;
        private readonly IQueueServiceFactory _queueServiceFactory;
        private readonly IConfigService<ProcessorConfiguration> _configService;
        //private readonly IObjectTrackingService _objectService;
        
        private readonly ObjectTrackingService _objectService;
        private readonly ObjectTrackingRepository _objectRespository;
        private readonly ObjectTrackingRepositorySettings _objectTrackingRepositorySettings;

        //private readonly IAuditService _auditService;
        private readonly AuditService _auditService;
        private readonly AuditRepository _auditRespository;
        private readonly AuditRespositorySettings _auditRespositorySettings;

        private readonly IModelValidatorFactory _modelValidatorFactory;

        private readonly IServicePrincipalProcessor _processor;

        private readonly ILogger<ServicePrincipalProcessor> _spLogger;

        private readonly ILogger<GraphDeltaProcessor> _gLogger;

        private readonly ILogger<AuditService> _aSLogger;
        private readonly ILogger<AuditRepository> _aRLogger;


        private readonly ILogger<ObjectTrackingService> _oTSLogger;
        private readonly ILogger<ObjectTrackingRepository> _oTRLogger;

        

        private readonly IServiceProvider _serviceProvider;

        public readonly IConfigurationRoot _config;
        public GraphDeltaProcessorFunctionsTests()
        {

            _config = new ConfigurationBuilder().AddJsonFile("appconfig.json").Build();


            // TODO: Need to add an interfaces for these so we can mock them or come up with another way to instantiate 
            // for testing. As it is right now the substitution won't work because the
            // constructors will actually get called and GraphHelperBase will try to create a graph client.

            //_processorResolver = Substitute.For<ProcessorResolver>();
            _secretClient = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<IGraphHelper<ServicePrincipal>>();

            
            _serviceProvider = Substitute.For<IServiceProvider>();
            _spLogger = Substitute.For<ILogger<ServicePrincipalProcessor>>();

            _gLogger = Substitute.For<ILogger<GraphDeltaProcessor>>();

            //********************************************************

            //TODO: BuildConfiguration(); <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            _servicePrincipalProcessorSettings = new ServicePrincipalProcessorSettings(_secretClient)// need this to setup queues
            {
                //QueueConnectionString = config[Constants.SPStorageConnectionString],
                //EvaluateQueueName = config[Constants.EvaluateQueueAppSetting.Trim('%')],
                //UpdateQueueName = config[Constants.UpdateQueueAppSetting.Trim('%')],
                //ConfigurationId = config["configId"].ToGuid(Guid.Parse("02a54ac9-441e-43f1-88ee-fde420db2559")),
                //VisibilityDelayGapSeconds = config["visibilityDelayGapSeconds"].ToInt(8),
                //QueueRecordProcessThreshold = config["queueRecordProcessThreshold"].ToInt(10),
                //AADUpdateMode = config["aadUpdateMode"].As<UpdateMode>(UpdateMode.Update)
            };

            _modelValidatorFactory = Substitute.For<IModelValidatorFactory>();
            _configService = Substitute.For<IConfigService<ProcessorConfiguration>>();

            //_auditService = Substitute.For<IAuditService>();*******************************************************
            _auditRespositorySettings = new AuditRespositorySettings(_secretClient)
            {
                //Uri = config[Constants.CosmosDBURLName],
                //Key = config[Constants.CosmosDBKeyName],
                //DatabaseName = config[Constants.CosmosDBDatabaseName],
                //CollectionName = config[Constants.CosmosDBAuditCollectionName]
            };
            _auditRespository = new AuditRepository(_auditRespositorySettings, _aRLogger);
            _auditService = new AuditService(_auditRespository, _aSLogger);


            //_objectService = Substitute.For<IObjectTrackingService>();************************************************************
            _objectTrackingRepositorySettings = new ObjectTrackingRepositorySettings(_secretClient)
            {
                //Uri = config[Constants.CosmosDBURLName],
                //Key = config[Constants.CosmosDBKeyName],
                //DatabaseName = config[Constants.CosmosDBDatabaseName],
                //CollectionName = config[Constants.CosmosDBObjectTrackingCollectionName]
            };
            _objectRespository = new ObjectTrackingRepository(_objectTrackingRepositorySettings, _oTRLogger);
            _objectService = new ObjectTrackingService(_objectRespository, _auditService, _oTSLogger);
            


            _queueServiceFactory = Substitute.For<IQueueServiceFactory>();
            

            //*********************************************************

            //_processor = Substitute.For<IServicePrincipalProcessor>();



            _processor = new ServicePrincipalProcessor(_servicePrincipalProcessorSettings, _graphHelper, _queueServiceFactory,
                        _configService, _objectService, _auditService, _modelValidatorFactory, _spLogger);

            _graphDeltaProcessor = new GraphDeltaProcessor(_serviceProvider, _processor, _gLogger, true);
        }

        /*
        private void BuildConfiguration()
        {
            // CONFIGURATION
            var serviceProvider = builder.Services.BuildServiceProvider();
            var envName = builder.GetContext().EnvironmentName;
            var appDirectory = builder.GetContext().ApplicationRootPath;
            var defaultConfig = serviceProvider.GetRequiredService<IConfiguration>();

            // order dependent.  Environment settings should override local configuration
            //  The reasoning for the order is a local config file is more difficult to change
            //  than an environment setting.  KeyVault settings should override any previous setting.
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(appDirectory)
                .AddJsonFile($"appsettings.{envName}.json", true)
                .AddConfiguration(defaultConfig)
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            // file only exists on local dev machine, so treat these as dev machine overrides
            //  the environment must be set to Development for this file to be even considered!
            if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                configBuilder
                    .AddJsonFile($"appsettings.Development.json", true);
            }

            var hostConfig = configBuilder.Build();
            //builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), hostConfig));
        }
        */

        [Fact]
        public void FunctionsTestScaffolding()
        {
            //TODO: This is basically scaffolding for the unit tests
            //for our functions
            Assert.True(true);
        }

        [Fact]
        public async void FunctionEvaluateTest()
        {
            using var commonFunctions = new CommonFunctions(_config);

            CloudQueueMessage  cloudQueueMessage = new CloudQueueMessage(commonFunctions.GetTestMessageContent(TestCase.TC1));


            //this also works 
            Task thisTaks = Task.Run (() => _graphDeltaProcessor.Evaluate(cloudQueueMessage, _gLogger));
            thisTaks.Wait();
          

            Assert.True(true);
        }

     
    }
}
