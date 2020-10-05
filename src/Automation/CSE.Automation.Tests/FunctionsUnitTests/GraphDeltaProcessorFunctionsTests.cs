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
        private readonly IServicePrincipalProcessor _processor;

        public GraphDeltaProcessorFunctionsTests()
        {
            _credService = Substitute.For<ICredentialService>();
            _secretService = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<ServicePrincipalGraphHelper>();
            _processor = Substitute.For<IServicePrincipalProcessor>();

            _subject = new GraphDeltaProcessor(_secretService, _graphHelper, _processor);
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
