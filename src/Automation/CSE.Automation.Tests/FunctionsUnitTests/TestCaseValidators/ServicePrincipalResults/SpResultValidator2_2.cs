using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator2_2 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator2_2(string savedServicePrincipalAsString, ServicePrincipal newServicePrincipal, TestCase testCase) : base(savedServicePrincipalAsString, newServicePrincipal, testCase)
        {
        }

        public override bool Validate()
        {
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            return SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
