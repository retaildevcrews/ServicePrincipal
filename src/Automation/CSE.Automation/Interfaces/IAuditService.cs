using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.OData.UriParser;

namespace CSE.Automation.Interfaces
{
    public interface IAuditService
    {
        Task PutFail(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null);

        Task PutPass(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null);

        Task PutIgnore(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null);

        Task PutChange(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string message = null, DateTimeOffset? auditTime = null);
    }
}
