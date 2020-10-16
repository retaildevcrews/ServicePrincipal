using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IConfigService<TConfig> where TConfig: class
    {
        Task<TConfig> GetByIdAsync(string id);
        Task<TConfig> ReplaceDocumentAsync(string id, TConfig newDocument);
        Task<TConfig> CreateDocumentAsync(TConfig newDocument, PartitionKey partitionKey);
        Task<TConfig> UpsertDocumentAsync(TConfig newDocument, PartitionKey partitionKey);
        Task<bool> DoesExistsAsync(string id);
        Task<TConfig> DeleteDocumentAsync(string id, PartitionKey partitionKey);
        
    }
}
