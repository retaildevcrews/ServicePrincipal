using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Graph;
using NSubstitute;
using Xunit;
using CSE.Automation.DataAccess;
using CSE.Automation.Processors;

namespace CSE.Automation.Tests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor _subject;
        private readonly ICredentialService _credService;
        private readonly ISecretClient _secretService;
        private readonly ServicePrincipalGraphHelper _graphHelper;
        private readonly DALResolver _DALResolver;
        private readonly ProcessorResolver _processorResolver;

        public GraphDeltaProcessorFunctionsTests()
        {
            _credService = Substitute.For<ICredentialService>();
            _secretService = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<ServicePrincipalGraphHelper>();
            _DALResolver = Substitute.For<DALResolver>();
            _processorResolver = Substitute.For<ProcessorResolver>();

            _subject = new GraphDeltaProcessor(_secretService, _credService, _graphHelper, _DALResolver, _processorResolver);
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
