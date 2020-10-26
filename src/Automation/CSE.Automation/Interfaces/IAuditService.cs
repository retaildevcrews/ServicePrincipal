using System;
using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IAuditService
    {
        Task PutFail(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string reason, DateTimeOffset? auditTime = null);

        Task PutPass(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string reason, DateTimeOffset? auditTime = null);

        Task PutIgnore(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string reason, DateTimeOffset? auditTime = null);

        Task PutChange(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason, DateTimeOffset? auditTime = null);

        //Task PutFailThenChange(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason, DateTimeOffset? auditTime = null);
    }
}
