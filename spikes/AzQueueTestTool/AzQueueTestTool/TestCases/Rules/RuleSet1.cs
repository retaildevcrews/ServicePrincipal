﻿using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet1 : RuleSetBase, IRuleSet
    {
        public RuleSet1(List<ServicePrincipal> targetServicePrincipals) : base (targetServicePrincipals)
        {
        }

        public override void Execute()
        {
            //-set owners 
            //-populated Notes field with owners AAD emails

            GraphHelper.ClearOwners(ServicePrincipals);
           
            Task task = GraphHelper.SetOwnersAsync(ServicePrincipals);

            task.Wait();
          
            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(ServicePrincipals);

       }

    }

}
