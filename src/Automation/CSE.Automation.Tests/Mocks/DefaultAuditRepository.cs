using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;

namespace CSE.Automation.Tests.Mocks
{
    internal class DefaultAuditRepository : IAuditRepository
    {
        public List<AuditEntry> Data = new List<AuditEntry>();

        public async Task<bool> Test()
        {
            return true;
        }

        public async Task Reconnect(bool force = false)
        {
            await Task.CompletedTask;
        }

        public string Id { get; }
        public async Task<AuditEntry> GetByIdAsync(string id, string partitionKey)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemResponse<AuditEntry>> GetByIdWithMetaAsync(string id, string partitionKey)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AuditEntry>> GetPagedAsync(string q, int offset = 0, int limit = 0)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AuditEntry>> GetAllAsync(TypeFilter filter = TypeFilter.Any)
        {
            throw new NotImplementedException();
        }

        public string GenerateId(AuditEntry entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }

        public async Task<AuditEntry> ReplaceDocumentAsync(string id, AuditEntry newDocument, ItemRequestOptions reqOptions = null)
        {
            throw new NotImplementedException();
        }

        public async Task<AuditEntry> CreateDocumentAsync(AuditEntry newDocument)
        {
            Data.Add(newDocument);
            return await Task.FromResult(newDocument);
        }

        public async Task<AuditEntry> UpsertDocumentAsync(AuditEntry newDocument)
        {
            throw new NotImplementedException();
        }

        public async Task<AuditEntry> DeleteDocumentAsync(string id, string partitionKey)
        {
            throw new NotImplementedException();
        }

        public string DatabaseName => "default";
        public string CollectionName => "default";
    }
}
