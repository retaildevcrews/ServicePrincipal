using System;
using System.Collections.Generic;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator3 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator3(string savedServicePrincipalAsString, InputGenerator inputGenerator, ActivityContext activityContext) 
                                : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            bool servicePrincipalPass = SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);

            List<string> targetQueueMessages = new List<string> () {"Update Notes from Owners"};

            bool messageFound = DoesMessageExistInUpdateQueue(targetQueueMessages);

            return (servicePrincipalPass && messageFound);

        }
    }
}
