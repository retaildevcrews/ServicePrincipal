using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
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
