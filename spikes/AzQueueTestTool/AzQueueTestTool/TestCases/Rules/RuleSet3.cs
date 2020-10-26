using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet3 : IRuleSet
    {
        //serviv
        public CaseId TestCaseId { get => CaseId.TC3; }
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => true;

        public bool ValidNotes => false;

        public void Execute(List<ServicePrincipal> targetServicePrincipals)
        {

            //-set owners 
            //-populated Notes field with valid emails other that AAD emails

            GraphHelper.ClearOwners(targetServicePrincipals);

            Task task =  GraphHelper.SetOwnersAsync(targetServicePrincipals);
            task.Wait();
            GraphHelper.UpdateNotesFieldWithValidEmail(targetServicePrincipals);
        }
    }
}
