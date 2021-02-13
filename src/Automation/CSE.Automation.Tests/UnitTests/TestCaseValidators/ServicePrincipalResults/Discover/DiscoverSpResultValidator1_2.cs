using CSE.Automation.Model;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalResults.Discover
{
    internal class DiscoverSpResultValidator1_2 : SpResultValidatorBase, ISpResultValidator
    {

        public DiscoverSpResultValidator1_2(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext, false)
        {
        }

        public override bool Validate()
        {
            // We check for messages in Evaluate queue for Discover Test cases.
            int messageFoundCount = GetMessageCountInEvaluateQueueFor(this.DisplayNamePatternFilter);
            return messageFoundCount == 0;

        }
    }
}
