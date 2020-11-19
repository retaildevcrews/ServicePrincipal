// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Config;
using CSE.Automation.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CSE.Automation.DataAccess
{
    internal abstract class CosmosDBRepository<TEntity> : ICosmosDBRepository<TEntity>, IDisposable
                                                    where TEntity : class
    {
        private const string PagedOffsetString = " offset {0} limit {1}";
        private readonly ICosmosDBSettings settings;
        private readonly ILogger logger;
        private readonly CosmosConfig options;
        private static CosmosClient client;
        private Container container;
        private ContainerProperties containerProperties;
        private PropertyInfo partitionKeyPI;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="settings">Instance of settings for a ComosDB</param>
        /// <param name="logger">Instance of logger.</param>
        protected CosmosDBRepository(ICosmosDBSettings settings, ILogger logger)
        {
            this.settings = settings;
            this.logger = logger;

            this.options = new CosmosConfig
            {
                MaxRows = MaxPageSize,
                Timeout = CosmosTimeout,
            };
        }

        public int DefaultPageSize { get; set; } = 100;
        public int MaxPageSize { get; set; } = 1000;
        public int CosmosTimeout { get; set; } = 60;
        public int CosmosMaxRetries { get; set; } = 10;
        public abstract string CollectionName { get; }
        public string DatabaseName => this.settings.DatabaseName;
        private object lockObj = new object();

        // NOTE: CosmosDB library currently wraps the Newtonsoft JSON serializer.  Align Attributes and Converters to Newtonsoft on the domain models.
        private CosmosClient Client => client ??= new CosmosClientBuilder(this.settings.Uri, this.settings.Key)
                                                    .WithRequestTimeout(TimeSpan.FromSeconds(CosmosTimeout))
                                                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(CosmosTimeout), CosmosMaxRetries)
                                                    .WithSerializerOptions(new CosmosSerializationOptions
                                                    {
                                                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                                                        Indented = false,
                                                        IgnoreNullValues = true,
                                                    })
                                                    .Build();
        private Container Container
        {
            get
            {
                lock (lockObj)
                {
                    return this.container ??= GetContainer(Client);
                }
            }
        }

        public abstract string GenerateId(TEntity entity);

        /// <summary>
        /// Recreate the Cosmos Client / Container (after a key rotation)
        /// </summary>
        /// <param name="force">force reconnection even if no params changed</param>
        /// <returns>Task</returns>
        public async Task Reconnect(bool force = false)
        {
            if (force || this.container.Id != this.CollectionName)
            {
                // open and test a new client / container
                client = null;
                if (await Test().ConfigureAwait(true) == false)
                {
                    logger.LogError($"Failed to reconnect to CosmosDB {this.settings.DatabaseName}:{this.CollectionName}");
                }
            }
        }

        public virtual PartitionKey ResolvePartitionKey(TEntity entity)
        {
            try
            {
                var value = new PartitionKey(this.partitionKeyPI.GetValue(entity).ToString());
                return value;
            }
            catch (Exception ex)
            {
                ex.Data["partitionKeyPath"] = this.containerProperties.PartitionKeyPath;
                ex.Data["entityType"] = typeof(TEntity);
                throw;
            }
        }

        public async Task<bool> Test()
        {
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ArgumentException($"CosmosCollection cannot be null");
            }

            // open and test a new client / container
            try
            {
                var containers = await GetContainerNames().ConfigureAwait(false);
                var containerNames = string.Join(',', containers);
                logger.LogDebug($"Test {this.Id} -- '{containerNames}'");
                if (containers.Any(x => x == this.CollectionName) == false)
                {
                    throw new ApplicationException();  // use same error path
                }

                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                logger.LogError(ex, $"Failed to find collection in CosmosDB {this.settings.DatabaseName}:{this.CollectionName}");
                return false;
            }
        }

        public string Id => $"{DatabaseName}:{CollectionName}";

        /// <summary>
        /// Given a document id and its partition value, retrieve the document, if it exists.
        /// </summary>
        /// <param name="id">Id of the document.</param>
        /// <param name="partitionKey">Value of the partitionkey for the document.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> GetByIdAsync(string id, string partitionKey)
        {
            var result = await GetByIdWithMetaAsync(id, partitionKey).ConfigureAwait(false);
            return result?.Resource;
        }

        public async Task<ItemResponse<TEntity>> GetByIdWithMetaAsync(string id, string partitionKey)
        {
            ItemResponse<TEntity> entityWithMeta = null;
            try
            {
                var result = await this.Container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);

                entityWithMeta = result;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // swallow exception
            }

            return entityWithMeta;
        }

        public async Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument, ItemRequestOptions reqOptions)
        {
            return await this.Container.ReplaceItemAsync<TEntity>(newDocument, id, ResolvePartitionKey(newDocument), reqOptions).ConfigureAwait(false);
        }

        public async Task<TEntity> CreateDocumentAsync(TEntity newDocument)
        {
            return await this.Container.CreateItemAsync<TEntity>(newDocument, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        public async Task<TEntity> UpsertDocumentAsync(TEntity newDocument)
        {
            return await this.Container.UpsertItemAsync<TEntity>(newDocument, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        public async Task<TEntity> DeleteDocumentAsync(string id, string partitionKey)
        {
            return await this.Container.DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Using lower case with cosmos queries as tested.")]
        public async Task<IEnumerable<TEntity>> GetPagedAsync(string q, int offset = 0, int limit = Constants.DefaultPageSize)
        {
            string sql = q;

            if (limit < 1)
            {
                limit = Constants.DefaultPageSize;
            }
            else if (limit > Constants.MaxPageSize)
            {
                limit = Constants.MaxPageSize;
            }

            string offsetLimit = string.Format(CultureInfo.InvariantCulture, PagedOffsetString, offset, limit);

            if (!string.IsNullOrEmpty(q))
            {
                // convert to lower and escape embedded '
                q = q.Trim().ToLowerInvariant().Replace("'", "''", System.StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(q))
                {
                    // get actors by a "like" search on name
                    sql += string.Format(CultureInfo.InvariantCulture, $" and contains(m.textSearch, @q) ");
                }
            }

            sql += offsetLimit;

            QueryDefinition queryDefinition = new QueryDefinition(sql);

            if (!string.IsNullOrEmpty(q))
            {
                queryDefinition.WithParameter("@q", q);
            }

            return await InternalCosmosDBSqlQuery(queryDefinition).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(TypeFilter filter = TypeFilter.Any)
        {
            string sql = "select * from m";
            if (filter != TypeFilter.Any)
            {
                sql += $" m.objectType='{Enum.GetName(typeof(TypeFilter), filter)}'";
            }

            return await InternalCosmosDBSqlQuery(sql).ConfigureAwait(false);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Get a proxy to the container.
        /// </summary>
        /// <param name="client">An instance of <see cref="CosmosClient"/></param>
        /// <returns>An instance of <see cref="Container"/>.</returns>
        internal Container GetContainer(CosmosClient client)
        {
            try
            {
                var container = client.GetContainer(this.settings.DatabaseName, this.CollectionName);

                this.containerProperties = GetContainerProperties(container).Result;
                var partitionKeyName = this.containerProperties.PartitionKeyPath.TrimStart('/');
                this.partitionKeyPI = typeof(TEntity).GetProperty(partitionKeyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (this.partitionKeyPI is null)
                {
                    throw new ApplicationException($"Failed to find partition key property {partitionKeyName} on {typeof(TEntity).Name}.  Collection definition does not match Entity definition");
                }

                logger.LogDebug($"{CollectionName} partition key path {this.containerProperties.PartitionKeyPath}");

                return container;
            }
            catch (Exception ex)
            {
                var message = $"Failed to connect to CosmosDB {this.settings.DatabaseName}:{this.CollectionName}";
                logger.LogCritical(ex, message);
                throw new ApplicationException(message, ex);
            }
        }

        /// <summary>
        /// Get the properties for the container.
        /// </summary>
        /// <param name="container">Instance of a container or null.</param>
        /// <returns>An instance of <see cref="ContainerProperties"/> or null.</returns>
        protected async Task<ContainerProperties> GetContainerProperties(Container container = null)
        {
            return (await (container ?? Container).ReadContainerAsync().ConfigureAwait(false)).Resource;
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="TEntity">POCO type to which results are serialized and returned.</typeparam>
        /// <param name="sql">Query to be executed.</param>
        /// <param name="options">Query options</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        protected async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery(string sql, QueryRequestOptions options = null)
        {
            // run query
            var query = this.Container.GetItemQueryIterator<TEntity>(sql, requestOptions: options ?? this.options.QueryRequestOptions);

            var results = new List<TEntity>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="T">POCO type to which results are serialized and returned.</typeparam>
        /// <param name="queryDefinition">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        private async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery(QueryDefinition queryDefinition)
        {
            // run query
            var query = this.Container.GetItemQueryIterator<TEntity>(queryDefinition, requestOptions: this.options.QueryRequestOptions);

            var results = new List<TEntity>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        /// <summary>
        /// Query the database for all the containers defined and return a list of the container names.
        /// </summary>
        /// <returns>A list of container names present in the configured database.</returns>
        private async Task<IList<string>> GetContainerNames()
        {
            var containerNames = new List<string>();
            var database = this.Client.GetDatabase(this.settings.DatabaseName);
            using var iter = database.GetContainerQueryIterator<ContainerProperties>();
            while (iter.HasMoreResults)
            {
                var response = await iter.ReadNextAsync().ConfigureAwait(false);

                containerNames.AddRange(response.Select(c => c.Id));
            }

            return containerNames;
        }
    }
}
