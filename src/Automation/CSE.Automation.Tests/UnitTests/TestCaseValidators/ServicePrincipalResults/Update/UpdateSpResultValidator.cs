using System.Collections.Generic;
using CSE.Automation.Model;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalResults.Update
{
    internal class UpdateSpResultValidator : SpResultValidatorBase, ISpResultValidator
    {

        public UpdateSpResultValidator(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(NewServicePrincipal);
            if (ownersList.Count > 0 && !string.IsNullOrEmpty(NewServicePrincipal.Notes))
            {
                foreach (var ownerName in ownersList.Values)
                {
                    if (!NewServicePrincipal.Notes.Contains(ownerName))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }
    }
}
