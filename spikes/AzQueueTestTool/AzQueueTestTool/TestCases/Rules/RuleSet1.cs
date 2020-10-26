using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet1 : IRuleSet
    {
        
        public CaseId TestCaseId { get => CaseId.TC1; }
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => true;

        public bool ValidNotes => true;

        public void Execute(List<ServicePrincipal> targetServicePrincipals)
        {

            ServicePrincipals = targetServicePrincipals;

            GraphHelper.ClearOwners(targetServicePrincipals);

            //-set owners 
            Task task = GraphHelper.SetOwnersAsync(targetServicePrincipals);

            task.Wait();
            //-populated Notes field with owners AAD emails
            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(targetServicePrincipals);

            // other methos used for othes TC
            /*
            GraphHelper.UpdateNotesFieldWithValidEmail(targetServicePrincipals);
            GraphHelper.ClearNotesFiled(targetServicePrincipals);

            GraphHelper.ClearOwners(targetServicePrincipals);
            */


        }
    }
}
