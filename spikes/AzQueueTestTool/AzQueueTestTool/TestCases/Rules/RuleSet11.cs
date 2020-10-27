using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet11 : RuleSetBase, IRuleSet
    {
        public RuleSet11(List<ServicePrincipal> targetServicePrincipals) : base(targetServicePrincipals)
        {
        }

        public override void Execute()
        {
            //DO NOT set owners 
            // Empty out Notes field

            GraphHelper.ClearNotesField(ServicePrincipals);
            
            GraphHelper.ClearOwners(ServicePrincipals);

        }

    }
}
