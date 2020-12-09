using System;
using System.Collections.Generic;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator6 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator6(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            bool servicePrincipalPass = SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);

            List<ServicePrincipalUpdateAction> targetQueueMessages = new List<ServicePrincipalUpdateAction> () { ServicePrincipalUpdateAction.Update};

            bool messageFound = DoesMessageExistInUpdateQueue(targetQueueMessages);

            return (servicePrincipalPass && messageFound);

        }
    }
}
