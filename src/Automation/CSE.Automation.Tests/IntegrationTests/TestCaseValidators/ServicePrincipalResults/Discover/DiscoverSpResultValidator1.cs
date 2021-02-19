using CSE.Automation.Model;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalResults.Discover
{
    internal class DiscoverSpResultValidator1 : SpResultValidatorBase, ISpResultValidator
    {

        public DiscoverSpResultValidator1(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext, false)
        {
        }

        public override bool Validate()
        {
            // The Max number of SPs queried by Delta request for testing purposes is 100. See ServicePrincipalGraphHelper.GetFilterString
            // So we will only try to get up to 100 SPs for a given Prefix
            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{this.DisplayNamePatternFilter}", 100).Result;

            // We check for messages in Evaluate queue for Discover Test cases.
            int messageFoundCount = GetMessageCountInEvaluateQueueFor(this.DisplayNamePatternFilter);

            return servicePrincipalList.Count == messageFoundCount;

        }
    }
}
