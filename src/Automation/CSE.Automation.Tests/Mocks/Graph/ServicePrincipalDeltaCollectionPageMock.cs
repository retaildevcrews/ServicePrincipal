using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;

namespace CSE.Automation.Tests.Mocks.Graph
{
    internal class ServicePrincipalDeltaCollectionPageMock : IServicePrincipalDeltaCollectionPage
    {
        public IEnumerator<ServicePrincipal> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ServicePrincipal item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ServicePrincipal item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ServicePrincipal[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ServicePrincipal item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(ServicePrincipal item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ServicePrincipal item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ServicePrincipal this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IList<ServicePrincipal> CurrentPage { get; set;  }
        public IDictionary<string, object> AdditionalData { get; set; }
        public void InitializeNextPageRequest(IBaseClient client, string nextPageLinkString)
        {
            throw new NotImplementedException();
        }

        public IServicePrincipalDeltaRequest NextPageRequest { get; set;  }
    }
}
