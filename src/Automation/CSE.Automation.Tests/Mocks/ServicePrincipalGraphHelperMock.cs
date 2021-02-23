using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.Mocks
{
    internal class ServicePrincipalGraphHelperMock : IServicePrincipalGraphHelper
    {
        public Dictionary<string, ServicePrincipal> Data { get; private set; } = new Dictionary<string, ServicePrincipal>();

        public static ServicePrincipalGraphHelperMock Create()
        {
            return new ServicePrincipalGraphHelperMock();
        }

        public ServicePrincipalGraphHelperMock WithData(ServicePrincipal[] data)
        {
            if (data != null)
            {
                this.Data = data.ToDictionary(x => x.Id);
            }

            return this;
        }

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
