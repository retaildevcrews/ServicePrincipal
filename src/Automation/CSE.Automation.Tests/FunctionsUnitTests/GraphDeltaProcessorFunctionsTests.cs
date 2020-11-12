using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Processors;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using NSubstitute;
using System;
using Xunit;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor subject;

        private readonly ISecretClient secretClient;
        private readonly IGraphHelper<ServicePrincipal> graphHelper;
        private readonly IServicePrincipalProcessor processor;
        private readonly IActivityService activityService;
        IServiceProvider serviceProvider;
        ILogger<GraphDeltaProcessor> logger;

        public GraphDeltaProcessorFunctionsTests()
        {
            // TODO: Need to add an interfaces for these so we can mock them or come up with another way to instantiate 
            // for testing. As it is right now the substitution won't work because the
            // constructors will actually get called and GraphHelperBase will try to create a graph client.
            // processorResolver = Substitute.For<ProcessorResolver>();
            this.secretClient = Substitute.For<ISecretClient>();
            this.graphHelper = Substitute.For<IGraphHelper<ServicePrincipal>>();
            this.processor = Substitute.For<IServicePrincipalProcessor>();
            this.serviceProvider = Substitute.For<IServiceProvider>();
            this.logger = Substitute.For<ILogger<GraphDeltaProcessor>>();
            this.activityService = Substitute.For<IActivityService>();
            this.subject = new GraphDeltaProcessor(serviceProvider, activityService, processor, this.logger);
        }

        [Fact]
        public void FunctionsTestScaffolding()
        {
            // TODO: This is basically scaffolding for the unit tests
            // for our functions
            Assert.True(true);
        }
    }
}
