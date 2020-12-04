using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    public interface IRuleSet
    {
        bool HasOwners {get; set;}
        List<ServicePrincipal> ServicePrincipals { get; }
        List<User> AADUsers { get; }
        void Execute();
    }
}
