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
            // TODO: Need to add an interfaces for these so we can mock them or come up with another way to instantiate 
            // for testing. As it is right now the substitution won't work because the
            // constructors will actually get called and GraphHelperBase will try to create a graph client.
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
