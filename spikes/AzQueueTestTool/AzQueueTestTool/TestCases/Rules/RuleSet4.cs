﻿using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;

namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet4 : RuleSetBase, IRuleSet
    {
        public RuleSet4(List<ServicePrincipal> targetServicePrincipals) : base(targetServicePrincipals)
        {
        }

        public override void Execute()
        {
            //-DO NOT set owners
            //- populated Notes field with valid emails other that AAD emails
            GraphHelper.ClearOwners(ServicePrincipals);

            GraphHelper.UpdateNotesFieldWithValidEmail(ServicePrincipals);

        }

    }
}
