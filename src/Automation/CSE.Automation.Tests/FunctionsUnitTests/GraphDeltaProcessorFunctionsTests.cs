using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;
using NSubstitute;
using Xunit;

namespace CSE.Automation.Tests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor _subject;
        private readonly ICredentialService _credService;
        private readonly ISecretClient _secretService;

        private readonly IGraphHelper _graphHelper;
        private readonly IDALResolver _DALResolver;

        public GraphDeltaProcessorFunctionsTests()
        {
            _credService = Substitute.For<ICredentialService>();
            _secretService = Substitute.For<ISecretClient>();
            _graphHelper = Substitute.For<IGraphHelper>();
            _DALResolver = Substitute.For<IDALResolver>();

            _subject = new GraphDeltaProcessor(_secretService, _credService, _graphHelper, _DALResolver);
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
