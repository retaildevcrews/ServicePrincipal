using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    public interface IRuleSet
    {
        CaseId TestCaseId { get; }
        bool ValidOwners { get; }
        bool ValidNotes { get; }
        List<ServicePrincipal> ServicePrincipals { get; set; }
        void CreateServicePrincipals();
    }
}
