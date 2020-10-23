using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet2 : IRuleSet
    {
        //serviv
        public CaseId TestCaseId { get => CaseId.TC2; }
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => false;

        public bool ValidNotes => true;

        public void Execute(List<ServicePrincipal> targetServicePrincipals)
        {

            //-DO NOT set owners 
            //-populated Notes field with owners AAD emails

            GraphHelper.ClearOwners(targetServicePrincipals);
            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(targetServicePrincipals);



        }
    }
}
