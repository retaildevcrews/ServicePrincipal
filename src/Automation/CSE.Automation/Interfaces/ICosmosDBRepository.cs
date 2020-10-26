using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{

    public interface ICosmosDBRepository<TEntity> : IRepository where TEntity : class
    {
        Task<TEntity> GetByIdAsync(string id, string partitionKey);
        Task<IEnumerable<TEntity>> GetPagedAsync(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<TEntity>> GetAllAsync(TypeFilter filter = TypeFilter.any);
        string GenerateId(TEntity entity);
        Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument);
        Task<TEntity> CreateDocumentAsync(TEntity newDocument);
        //Task<bool> DoesExistsAsync(string id);
        Task<TEntity> UpsertDocumentAsync(TEntity newDocument);
        Task<TEntity> DeleteDocumentAsync(string id, string partitionKey);
        string DatabaseName { get; }
        string CollectionName { get; }
    }
}
