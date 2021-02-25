using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.Mocks
{
    internal class NoopServicePrincipalGraphHelper : IServicePrincipalGraphHelper
    {
        public Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config)
        {
            throw new NotImplementedException();
        }

        public Task<(ServicePrincipal, IList<User>)> GetEntityWithOwners(string id)
        {
            throw new NotImplementedException();
        }

        public Task PatchGraphObject(ServicePrincipal entity)
        {
            throw new NotImplementedException();
        }

        public Task<Application> GetApplicationWithOwners(string appId)
        {
            throw new NotImplementedException();
        }


    }
}
