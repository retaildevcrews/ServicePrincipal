using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;


namespace AzQueueTestTool.TestCases.Rules
{
    abstract class RuleSetBase : IRuleSet
    {
        public bool HasOwners { get; set; }
        public List<ServicePrincipal> ServicePrincipals { get; }
        public List<User> AADUsers { get; }

        public RuleSetBase(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers)
        {
            ServicePrincipals = targetServicePrincipals;
            AADUsers = targetUsers;
        }

        public virtual void Execute()
        {
            HasOwners = !GraphHelper.ClearOwners(ServicePrincipals);
            GraphHelper.ClearNotesField(ServicePrincipals);
        }
    }
}
