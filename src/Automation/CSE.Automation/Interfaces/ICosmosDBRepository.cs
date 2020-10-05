using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CSE.Automation.Interfaces
{
    public interface ICosmosDBRepository
    {
        Task<T> GetById<T>(string Id, string partitionKey);
        Task<IEnumerable<T>> GetPagedAsync<T>(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<T>> GetAllAsync<T>(TypeFilter filter= TypeFilter.any);
        Task Reconnect(bool force = false);
        Task<bool> Test();
        PartitionKey ResolvePartitionKey(string entityId);
        string GenerateId<TEntity>(TEntity entity) where TEntity : class;
        string CollectionName { get; }
    }
}
