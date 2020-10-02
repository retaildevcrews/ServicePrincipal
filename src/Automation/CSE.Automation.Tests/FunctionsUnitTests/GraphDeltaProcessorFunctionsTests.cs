using NSubstitute;
using Xunit;

using CSE.Automation.Processors;

namespace CSE.Automation.Tests
{
    public class GraphDeltaProcessorFunctionsTests
    {
        private readonly GraphDeltaProcessor _subject;
        private readonly ProcessorResolver _processorResolver;

        public GraphDeltaProcessorFunctionsTests()
        {
            _processorResolver = Substitute.For<ProcessorResolver>();
            _subject = new GraphDeltaProcessor(_processorResolver);
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
