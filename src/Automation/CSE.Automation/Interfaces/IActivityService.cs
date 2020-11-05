using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.OData.UriParser;

namespace CSE.Automation.Interfaces
{
    internal interface IActivityService
    {
        Task<ActivityHistory> Put(ActivityHistory document);
        Task<ActivityHistory> Get(string id);
        ActivityContext CreateContext(string name, string correlationId = null, bool withTracking = false);
    }
}
