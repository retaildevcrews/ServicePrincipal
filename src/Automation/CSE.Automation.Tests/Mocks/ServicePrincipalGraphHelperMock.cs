using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.Mocks
{
    internal class ServicePrincipalGraphHelperMock : IServicePrincipalGraphHelper
    {
        public async Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config)
        {
            throw new NotImplementedException();
        }

        public async Task<ServicePrincipal> GetEntityWithOwners(string id)
        {
            throw new NotImplementedException();
        }

        public async Task PatchGraphObject(ServicePrincipal entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Application> GetApplicationWithOwners(string appId)
        {
            throw new NotImplementedException();
        }
    }
}
