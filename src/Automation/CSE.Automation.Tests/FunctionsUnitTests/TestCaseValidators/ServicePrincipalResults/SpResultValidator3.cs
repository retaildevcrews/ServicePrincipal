using System;
using System.Collections.Generic;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator3 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator3(string savedServicePrincipalAsString, ServicePrincipal newServicePrincipal, TestCase testCase) : base(savedServicePrincipalAsString, newServicePrincipal, testCase)
        {
        }

        public override bool Validate()
        {
            // Will not pass "Notes set to Owners" validation since ServicePrincipal Object is not updated in function Evaluate,
            // a ServicePrincipalUpdateCommand message  is generated to be picked up by Function Update Queue.

      
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            bool servicePrincipalPass = SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);


            // TODO should Validate Update Queue instead?    <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<




            return (servicePrincipalPass) ;


        }
    }
}
