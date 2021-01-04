using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Azure.Cosmos.Linq;
using CSE.Automation.DataAccess;
using System.Threading.Tasks;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class GraphDeltaProcessorWrapper : IDisposable
    {
        public string ConfigId { get; }

        public GraphDeltaProcessor GraphDeltaProcessorInstance { get;  }


        public GraphDeltaProcessorWrapper(GraphDeltaProcessor graphDeltaProcessor, Guid configId)
        {
            GraphDeltaProcessorInstance = graphDeltaProcessor;
            ConfigId = configId.ToString();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        internal void DeleteConfigItem()
        {
            throw new NotImplementedException();
        }
    }

    internal class GraphDeltaProcessorWrapperFactory : IDisposable
    { 

        private IGraphHelper<ServicePrincipal> _graphHelper;
        private IQueueServiceFactory _queueServiceFactory;
        private IConfigService<ProcessorConfiguration> _configService;
        private IObjectTrackingService _objectService;
        private IAuditService _auditService;
        private IActivityService _activityService;
        private ILogger<ServicePrincipalProcessor> _spProcessorLogger;
        private VersionMetadata _versionMetadata;
        private IServiceProvider _serviceProvider;
        private IServicePrincipalProcessor _processor;
        private ILogger<GraphDeltaProcessor> _graphLogger;

        private IConfigurationRoot _config;
        private ISecretClient _secretClient;

        private ConfigRepository _configRespository;
        public GraphDeltaProcessorWrapperFactory(
            IGraphHelper<ServicePrincipal> graphHelper,
            IQueueServiceFactory queueServiceFactory,
            IConfigService<ProcessorConfiguration> configService,
            IObjectTrackingService objectService,
            IAuditService auditService,
            IActivityService activityService,
            ILogger<ServicePrincipalProcessor> splogger,
            VersionMetadata versionMetadata,
            IServiceProvider serviceProvider,
            IServicePrincipalProcessor processor,
            ILogger<GraphDeltaProcessor> graphlogger,
            IConfigurationRoot config, 
            ISecretClient secretClient,
            ConfigRepository configRespository
            )
        {
            _graphHelper = graphHelper;
            _queueServiceFactory = queueServiceFactory;
            _configService = configService;
            _objectService = objectService;
            _auditService = auditService;
            _activityService = activityService;
            _spProcessorLogger = splogger;
            _versionMetadata = versionMetadata;
            _serviceProvider = serviceProvider;
            _processor = processor;
            _graphLogger = graphlogger;
            _config = config;
            _secretClient = secretClient;
            _configRespository = configRespository;
        }

        public GraphDeltaProcessorWrapper GetNewGraphDeltaProcessorWrapper()
        {
            Guid assignedGuidId = Guid.NewGuid();

            ProcessorConfiguration config = _configService.Get(assignedGuidId.ToString(), ProcessorType.ServicePrincipal, ServicePrincipalProcessor.ConstDefaultConfigurationResourceName, true);

            if (config != null)
            {
                return new GraphDeltaProcessorWrapper(CreateNewGraphDeltaProcessor(assignedGuidId), assignedGuidId);
            }
            else
            {
                new InvalidDataException("Unable to create a new Configuration item");
                return null;
            }
        }

        private GraphDeltaProcessor CreateNewGraphDeltaProcessor(Guid assignedGuidId)
        {
            var modelValidatorFactory = _serviceProvider.GetService<IModelValidatorFactory>();

            ServicePrincipalProcessorSettings newServicePrincipalProcessorSettings = CreateNewServicePrincipalProcessorSettings(assignedGuidId);

            var newProcessor = new ServicePrincipalProcessor(newServicePrincipalProcessorSettings, _graphHelper, _queueServiceFactory, _configService,
                      _objectService, _auditService, _activityService, modelValidatorFactory, _spProcessorLogger);

            var newGraphDeltaProcessor = new GraphDeltaProcessor(_versionMetadata, _serviceProvider, _activityService, newProcessor, _graphLogger);
            return newGraphDeltaProcessor;
        }

        private ServicePrincipalProcessorSettings CreateNewServicePrincipalProcessorSettings(Guid assignedGuidId)
        {
            return new ServicePrincipalProcessorSettings(_secretClient)
            {
                QueueConnectionString = _config[Constants.SPStorageConnectionString],
                EvaluateQueueName = _config[Constants.EvaluateQueueAppSetting.Trim('%')],
                UpdateQueueName = _config[Constants.UpdateQueueAppSetting.Trim('%')],
                DiscoverQueueName = _config[Constants.DiscoverQueueAppSetting.Trim('%')],
                ConfigurationId = assignedGuidId,// New guid every time we create a new ServicePrincipalProcessorSettings object
                VisibilityDelayGapSeconds = _config["visibilityDelayGapSeconds"].ToInt(8),
                QueueRecordProcessThreshold = _config["queueRecordProcessThreshold"].ToInt(10),
                AADUpdateMode = _config["aadUpdateMode"].As<UpdateMode>(UpdateMode.Update)

            };
        }

        public void Dispose()
        {
           // throw new NotImplementedException();
        }

        internal void DeleteConfigItem(GraphDeltaProcessorWrapper graphDeltaProcessorWrapper)
        {
            Task<ProcessorConfiguration> deleteTask = Task.Run( () => _configRespository.DeleteDocumentAsync(graphDeltaProcessorWrapper.ConfigId, ProcessorType.ServicePrincipal.ToString() ));
            deleteTask.Wait();
            
        }
    }
}
