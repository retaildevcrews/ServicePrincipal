using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IAuditService
    {
        Task<AuditEntry> Put(object originalDocument, string actionType = "", string actionReason = "");
    }
}
