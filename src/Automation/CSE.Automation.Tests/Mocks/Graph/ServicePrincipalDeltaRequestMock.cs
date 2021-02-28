using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace CSE.Automation.Tests.Mocks.Graph
{
    internal class ServicePrincipalDeltaRequestMock : IServicePrincipalDeltaRequest
    {
        public List<Dictionary<string, ServicePrincipal>> Data { get; private set; } = new List<Dictionary<string, ServicePrincipal>>();
        public int CurrentPage = -1;

        public ServicePrincipalDeltaRequestMock WithData(ServicePrincipal[] page1, ServicePrincipal[] page2 = null)
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

        public HttpRequestMessage GetHttpRequestMessage()
        {
            throw new NotImplementedException();
        }

        public string ContentType { get; set; }
        public IList<HeaderOption> Headers { get; }
        public IBaseClient Client { get; }
        public string Method { get; }
        public string RequestUrl { get; }
        public IList<QueryOption> QueryOptions { get; }
        public IDictionary<string, IMiddlewareOption> MiddlewareOptions { get; }
        public async Task<IServicePrincipalDeltaCollectionPage> GetAsync()
        {
            var page = new ServicePrincipalDeltaCollectionPageMock();
            CurrentPage++;
            if (CurrentPage < this.Data.Count)
            {
                page.CurrentPage = this.Data[CurrentPage].Values.ToList();
                page.NextPageRequest = this;
                return await Task.FromResult(page);
            }

            page.NextPageRequest = null;
            page.AdditionalData = new Dictionary<string, object>() { { "@odata.deltaLink", "link" } };
            return await Task.FromResult(page);
        }

        public async Task<IServicePrincipalDeltaCollectionPage> GetAsync(CancellationToken cancellationToken)
        {
            return await GetAsync();
        }

        public IServicePrincipalDeltaRequest Expand(string value)
        {
            throw new NotImplementedException();
        }

        public IServicePrincipalDeltaRequest Select(string value)
        {
            throw new NotImplementedException();
        }

        public IServicePrincipalDeltaRequest Top(int value)
        {
            throw new NotImplementedException();
        }

        public IServicePrincipalDeltaRequest Filter(string value)
        {
            throw new NotImplementedException();
        }

        public IServicePrincipalDeltaRequest Skip(int value)
        {
            throw new NotImplementedException();
        }

        public IServicePrincipalDeltaRequest OrderBy(string value)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, ServicePrincipal> MergedData
        {
            get
            {
                return this.Data
                            .SelectMany(dict => dict.Values)
                            .ToDictionary(g => g.Id);
            }
        }

        public ServicePrincipal this[string id]
        {
            get
            {
                return MergedData.TryGetValue(id, out var sp) ? sp : null;
            }
        }
    }
}
