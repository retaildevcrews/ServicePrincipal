﻿using System;
using System.Collections.Generic;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;
using Newtonsoft.Json;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalResults
{
    internal class SpResultValidator5 : SpResultValidatorBase, ISpResultValidator
    {

        public SpResultValidator5(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            var newServicePrincipalAsString = JsonConvert.SerializeObject(NewServicePrincipal);

            List<ServicePrincipalUpdateAction> targetQueueMessages = new List<ServicePrincipalUpdateAction> () { ServicePrincipalUpdateAction.Update, ServicePrincipalUpdateAction.Revert};

            bool messageNotFound = DoesMessageExistInUpdateQueue(targetQueueMessages);

            return !messageNotFound && SavedServicePrincipalAsString.Equals(newServicePrincipalAsString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
