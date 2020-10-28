using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet7 : RuleSetBase, IRuleSet
    {
        public RuleSet7(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {
        }
    }
}
