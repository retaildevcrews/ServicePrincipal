using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    abstract class RuleSetBase : IRuleSet
    {
        public List<ServicePrincipal> ServicePrincipals { get; }

        public RuleSetBase(List<ServicePrincipal> targetServicePrincipals)
        {
            ServicePrincipals = targetServicePrincipals;
        }

        public abstract void Execute();
    }
}
