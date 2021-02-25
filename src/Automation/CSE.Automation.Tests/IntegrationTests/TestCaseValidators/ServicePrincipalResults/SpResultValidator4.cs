﻿using System;
using System.Collections.Generic;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;
using Newtonsoft.Json;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator4 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator4(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            bool servicePrincipalPass = SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);

            List<ServicePrincipalUpdateAction> targetQueueMessages = new List<ServicePrincipalUpdateAction> () { ServicePrincipalUpdateAction.Revert};

            bool messageFound = DoesMessageExistInUpdateQueue(targetQueueMessages);

            return (servicePrincipalPass && messageFound);

        }
    }
}
