using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet5 : RuleSetBase, IRuleSet
    {
        public RuleSet5(List<ServicePrincipal> targetServicePrincipals) : base(targetServicePrincipals)
        {
        }

        public override void Execute()
        {

            // DO NOT set owners 
            // populated Notes field with valid emails other tht AAD emails

            GraphHelper.ClearOwners(ServicePrincipals);

            GraphHelper.UpdateNotesFieldWithValidEmail(ServicePrincipals);

        }

    }
}
