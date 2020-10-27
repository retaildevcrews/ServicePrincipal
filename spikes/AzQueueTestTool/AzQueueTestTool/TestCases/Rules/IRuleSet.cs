using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    public interface IRuleSet
    {
        List<ServicePrincipal> ServicePrincipals { get; }
        void Execute();
    }
}
