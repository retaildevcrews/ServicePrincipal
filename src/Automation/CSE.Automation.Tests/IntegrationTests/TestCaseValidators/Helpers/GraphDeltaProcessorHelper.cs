using System;
using CSE.Automation.DataAccess;
using CSE.Automation.Graph;
using CSE.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.Helpers
{
    internal class GraphDeltaProcessorHelper : IDisposable
    {
        public GraphDeltaProcessor GraphDeltaProcessorInstance { get; }

        public ActivityService ActivityServiceInstance { get; }

        public ILogger<GraphDeltaProcessor> GraphLoggerInstance { get; }

        public IConfigurationRoot ConfigInstance { get; }

        public bool DeleteDynamicCreatedServicePrincipals { get; set; }

        public ConfigRepository ConfigRepositoryInstance { get; }

        public string MainTestCaseConfigId { get; }

        public GraphHelperSettings GraphHelperSettingsInstance {get;}

        public GraphDeltaProcessorHelper(GraphDeltaProcessor graphDeltaProcessor, ActivityService activityService, ILogger<GraphDeltaProcessor> graphLogger, 
                                        IConfigurationRoot config, ConfigRepository configRepository, string mainTestCaseConfigId, GraphHelperSettings graphHelperSettings)
        {
            GraphDeltaProcessorInstance = graphDeltaProcessor;
            ActivityServiceInstance = activityService;
            GraphLoggerInstance = graphLogger;
            ConfigInstance = config;
            ConfigRepositoryInstance = configRepository;
            MainTestCaseConfigId = mainTestCaseConfigId;
            GraphHelperSettingsInstance = graphHelperSettings;
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
