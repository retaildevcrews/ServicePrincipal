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
        private readonly GraphDeltaProcessor _subject;

        private readonly ISecretClient _secretClient;
        private readonly IGraphHelper<ServicePrincipal> _graphHelper;
        private readonly IServicePrincipalProcessor _processor;
        IServiceProvider _serviceProvider;
        ILogger _logger;

        public GraphDeltaProcessorFunctionsTests()
        {
            // TODO: Need to add an interfaces for these so we can mock them or come up with another way to instantiate 
            // for testing. As it is right now the substitution won't work because the
            // constructors will actually get called and GraphHelperBase will try to create a graph client.
            //_processorResolver = Substitute.For<ProcessorResolver>();
            _secretClient = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<IGraphHelper<ServicePrincipal>>();
            _processor = Substitute.For<IServicePrincipalProcessor>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            _logger = Substitute.For<ILogger>();

            _subject = new GraphDeltaProcessor(_serviceProvider, _processor, _logger);
        }

        [Fact]
        public void FunctionsTestScaffolding()
        {
            //TODO: This is basically scaffolding for the unit tests
            //for our functions
            Assert.True(true);
        }
    }
}
