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
        Task PutFail(string attributeName, string existingAttributeValue, string reason, ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null);

        Task PutPass(string attributeName, string existingAttributeValue, string reason, ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null);

        Task PutChange(string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason, ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null);

        Task PutFailThenChange(string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason, ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null);


    }
}
