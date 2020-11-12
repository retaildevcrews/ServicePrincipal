// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CSE.Automation.Interfaces
{
    public interface ICosmosDBRepository<TEntity> : IRepository
        where TEntity : class
    {
        Task<TEntity> GetByIdAsync(string id, string partitionKey);
        Task<ItemResponse<TEntity>> GetByIdWithMetaAsync(string id, string partitionKey);
        Task<IEnumerable<TEntity>> GetPagedAsync(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<TEntity>> GetAllAsync(TypeFilter filter = TypeFilter.Any);
        string GenerateId(TEntity entity);
        Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument, ItemRequestOptions reqOptions = null);
        Task<TEntity> CreateDocumentAsync(TEntity newDocument);
        Task<TEntity> UpsertDocumentAsync(TEntity newDocument);
        Task<TEntity> DeleteDocumentAsync(string id, string partitionKey);
        Task<IEnumerable<TEntity>> GetMostRecentAsync(string objectId, int limit = 1);
        string DatabaseName { get; }
        string CollectionName { get; }
    }
}
