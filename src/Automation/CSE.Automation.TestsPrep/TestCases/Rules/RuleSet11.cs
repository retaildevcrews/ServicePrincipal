using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace CSE.Automation.TestsPrep.TestCases.Rules
{
    internal class RuleSet11 : RuleSetBase, IRuleSet
    {
        public RuleSet11(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {
        }

    }
}
