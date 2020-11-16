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

      
            // TODO should Validate Update Queue instead?    <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            bool result = false;

            //var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            //bool same = SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);

            //ServicePrincipal savedServicePrincipal = JsonConvert.DeserializeObject<ServicePrincipal>(SavedServicePrincipalAsString);



            /*

                   Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(NewServicePrincipal);
                   if (ownersList.Count > 0 && !string.IsNullOrEmpty(NewServicePrincipal.Notes))
                   {
                       int validatedCount = 0;
                       foreach (var ownerName in ownersList.Values)
                       {
                           validatedCount++;
                           if (!NewServicePrincipal.Notes.Contains(ownerName))
                           {
                               break;  
                           }
                       }

                       result = true && ownersList.Values.Count == validatedCount;
                   }

                   */

            
            return result ;


        }
    }
}
