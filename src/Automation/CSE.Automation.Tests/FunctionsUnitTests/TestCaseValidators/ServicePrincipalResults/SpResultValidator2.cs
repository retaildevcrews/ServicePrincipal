using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator2 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator2(string savedServicePrincipalAsString, InputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);
            
            List<string> targetQueueMessages = new List<string> () {"Update Notes from Owners", "Revert to Last Known Good"};

            bool messageNotFound = DoesMessageExistInUpdateQueue(targetQueueMessages);

            return !messageNotFound && SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
