﻿using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;

namespace CSE.Automation.TestsPrep.TestCases.Rules
{
    internal class RuleSet4 : RuleSetBase, IRuleSet
    {
        public RuleSet4(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {
        }

        public override void Execute()
        {
            base.Execute();
            //-DO NOT set owners
            //- populated BusinessOwners field with valid emails other that AAD emails
            GraphHelper.ClearOwners(ServicePrincipals);

            GraphHelper.UpdateNotesFieldWithValidEmail(ServicePrincipals);

        }

    }
}
