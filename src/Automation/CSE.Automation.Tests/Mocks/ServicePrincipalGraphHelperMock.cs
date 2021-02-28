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
        public List<Dictionary<string, ServicePrincipal>> Data { get; private set; } = new List<Dictionary<string, ServicePrincipal>>();
        public int CurrentPage = -1;

        public static ServicePrincipalGraphHelperMock Create()
        {
            return new ServicePrincipalGraphHelperMock();
        }

        public ServicePrincipalGraphHelperMock WithData(ServicePrincipal[] page1, ServicePrincipal[] page2 = null)
        {
            if (page1 != null)
            {
                this.Data.Add(page1.ToDictionary(x => x.Id));

                if (page2 != null)
                {
                    this.Data.Add(page2.ToDictionary(x => x.Id));
                }
            }

            return this;
        }

        public async Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config)
        {
            var metrics = new GraphOperationMetrics();

            CurrentPage++;
            if (CurrentPage < this.Data.Count)
            {
                var result = this.Data[CurrentPage].Values;
                metrics.AdditionalData = "more data";
                return await Task.FromResult((metrics, result));
            }

            metrics.AdditionalData = null;
            return await Task.FromResult((metrics, new List<ServicePrincipal>()));
        }

        public async Task<(ServicePrincipal, IList<User>)> GetEntityWithOwners(string id)
        {
            ServicePrincipal model = null;
            IList<User> owners = null;

            if (CurrentPage >= 0 && CurrentPage < this.Data.Count)
            {
                this.Data[CurrentPage].TryGetValue(id, out model);
                if (model != null)
                {
                    owners = new List<User>();
                }
            }

            return await Task.FromResult((model, owners));
        }

        public Task PatchGraphObject(ServicePrincipal entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Application> GetApplicationWithOwners(string appId)
        {
            return await Task.FromResult((Application)null);
        }
    }
}
