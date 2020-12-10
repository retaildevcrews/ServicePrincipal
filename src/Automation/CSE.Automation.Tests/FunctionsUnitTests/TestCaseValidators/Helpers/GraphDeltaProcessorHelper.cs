using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers
{
    internal class GraphDeltaProcessorHelper : IDisposable
    {
        public GraphDeltaProcessor GraphDeltaProcessorInstance { get; }

        public ActivityService ActivityServiceInstance { get; }

        public ILogger<GraphDeltaProcessor> GraphLoggerInstance { get; }

        public IConfigurationRoot ConfigInstance { get; }

        public bool DeleteDynamicCreatedServicePrincipals { get; set; }

        public GraphDeltaProcessorHelper(GraphDeltaProcessor graphDeltaProcessor, ActivityService activityService, ILogger<GraphDeltaProcessor> graphLogger, IConfigurationRoot config)
        {
            GraphDeltaProcessorInstance = graphDeltaProcessor;
            ActivityServiceInstance = activityService;
            GraphLoggerInstance = graphLogger;
            ConfigInstance = config;
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
