using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CSE.Automation.Interfaces
{

    public interface ICosmosDBRepository<TEntity> : IRepository where TEntity : class
    {
        Task<TEntity> GetByIdAsync(string id);
        Task<IEnumerable<TEntity>> GetPagedAsync(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<TEntity>> GetAllAsync(TypeFilter filter = TypeFilter.any);
        PartitionKey ResolvePartitionKey(string entityId);
        string GenerateId(TEntity entity);
        Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument);
        Task<TEntity> CreateDocumentAsync(TEntity newDocument, PartitionKey partitionKey);
        Task<bool> DoesExistsAsync(string id);
        Task<TEntity> UpsertDocumentAsync(TEntity newDocument, PartitionKey partitionKey);
        Task<TEntity> DeleteDocumentAsync(string id, PartitionKey partitionKey);
        string DatabaseName { get; }
        string CollectionName { get; }
    }
}
