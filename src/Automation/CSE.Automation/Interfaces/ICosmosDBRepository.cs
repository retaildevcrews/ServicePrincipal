using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CSE.Automation.Interfaces
{
    public interface ICosmosDBRepository
    {
        Task<T> GetByIdAsync<T>(string id);
        Task<IEnumerable<T>> GetPagedAsync<T>(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<T>> GetAllAsync<T>(TypeFilter filter= TypeFilter.any);
        Task Reconnect(bool force = false);
        Task<bool> Test();
        PartitionKey ResolvePartitionKey(string entityId);
        string GenerateId<TEntity>(TEntity entity) where TEntity : class;
        Task<T> ReplaceDocumentAsync<T>(string id, T newDocument);
        Task<T> CreateDocumentAsync<T>(T newDocument, PartitionKey partitionKey);
        Task<bool> DoesExistsAsync(string id);

        string DatabaseName { get; }
        string CollectionName { get; }

    }
}
