using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.OData.UriParser;

namespace CSE.Automation.Interfaces
{
    internal interface IAuditService
    {
        Task<AuditEntry> Put(object originalDocument, string actionType = "", string actionReason = "");
        //Task Fail(ActivityContext context, string objectId, string attributeName, string attributeValue, string reason);


    }
    /*
    class blah
    {
        void func()
        {
            IAuditService svc;

            svc.Fail(context).ForObject(id).AndAttribute(attrName).AndValue(attrValue).WithReason(reason);

            svc.Fail(context, id, attrName, attrValue, reason);
        }
    }
    */
}
