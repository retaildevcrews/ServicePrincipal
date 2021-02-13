using System.Collections.Generic;
using System.Linq;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalResults.Update
{
    internal class UpdateSpResultValidator2 : SpResultValidatorBase, ISpResultValidator
    {

        public UpdateSpResultValidator2(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            List<string> assignedUsersAsList = (InputGeneratorInstance as UpdateInputGenerator).GetAssignedOwnersTestCase2();

            List<string> newNotesAsList = NewServicePrincipal.Notes.GetAsList();

            return newNotesAsList.Except(assignedUsersAsList).Count() == 0;

        }
    }
}
