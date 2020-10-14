using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CSE.Automation.Config;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.DataAccess
{

    class CosmosDBSettings : SettingsBase, ICosmosDBSettings
    {
        public CosmosDBSettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.CosmosDBURLName)]
        public string Uri => base.GetSecret();

        [Secret(Constants.CosmosDBKeyName)]
        public string Key => base.GetSecret();
        [Secret(Constants.CosmosDBDatabaseName)]
        public string DatabaseName => base.GetSecret();

        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.Uri)) throw new ConfigurationErrorsException($"{this.GetType().Name}: Uri is invalid");
            if (string.IsNullOrEmpty(this.Key)) throw new ConfigurationErrorsException($"{this.GetType().Name}: Key is invalid");
            if (string.IsNullOrEmpty(this.DatabaseName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: DatabaseName is invalid");
        }
    }

    abstract class CosmosDBRepository<TEntity> : ICosmosDBRepository<TEntity>, IDisposable where TEntity : class
    {
        const string pagedOffsetString = " offset {0} limit {1}";


        private readonly CosmosConfig _options;
        private static CosmosClient _client;
        private Container _container;
        private readonly ICosmosDBSettings _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        protected CosmosDBRepository(ICosmosDBSettings settings, ILogger logger)
        {
            if (settings.Uri == null)
            {
                throw new ArgumentException("settings.Uri cannot be null");
            }

            _settings = settings;
            _logger = logger;

            _options = new CosmosConfig
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
        public string DatabaseName => _settings.DatabaseName;

        CosmosClient Client => _client ??= new CosmosClient(_settings.Uri, _settings.Key, _options.CosmosClientOptions);
        private Container Container => _container ??= GetContainer(Client);

        public abstract string GenerateId(TEntity entity);
        public virtual PartitionKey ResolvePartitionKey(string entityId) => PartitionKey.Null;

        /// <summary>
        /// Recreate the Cosmos Client / Container (after a key rotation)
        /// </summary>
        /// <param name="force">force reconnection even if no params changed</param>
        /// <returns>Task</returns>
        public async Task Reconnect(bool force = false)
        {
            if (force || _container.Id != this.CollectionName)
            {

                // open and test a new client / container
                _client = null;
                if (await Test().ConfigureAwait(true) == false)
                {
                    _logger.LogError($"Failed to reconnect to CosmosDB {_settings.DatabaseName}:{this.CollectionName}");
                }

            }
        }

#if OLDCODE
        /// <summary>
        /// Open and test the Cosmos Client / Container / Query
        /// </summary>
        /// <param name="cosmosUrl">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <returns>An open and validated CosmosClient</returns>
        private CosmosClient OpenAndTestCosmosClient(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // validate required parameters
            if (cosmosUrl == null)
            {
                throw new ArgumentNullException(nameof(cosmosUrl));
            }

            if (string.IsNullOrEmpty(cosmosKey))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosKey not set correctly {cosmosKey}"));
            }

            if (string.IsNullOrEmpty(cosmosDatabase))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosDatabase not set correctly {cosmosDatabase}"));
            }

            if (string.IsNullOrEmpty(cosmosCollection))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosCollection not set correctly {cosmosCollection}"));
            }

            // open and test a new client / container
#pragma warning disable CA2000 // Dispose objects before losing scope.  Disabling as the container connection remains in scope.
            var c = new CosmosClient(cosmosUrl.AbsoluteUri, cosmosKey, _cosmosOptions.CosmosClientOptions);
#pragma warning restore CA2000 // Dispose objects before losing scope
            var con = c.GetContainer(cosmosDatabase, cosmosCollection);

            //TODO: commenting out for the moment.  Need a good way to test that doesn't require a document
            //await con.ReadItemAsync<dynamic>("action", new PartitionKey("0")).ConfigureAwait(false);

            return c;
        }

#endif


        public async Task<bool> Test()
        {
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ArgumentException($"CosmosCollection cannot be null");
            }

            // open and test a new client / container
            try
            {
                return (await GetContainerNames().ConfigureAwait(false)).Any(x => x == this.CollectionName);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogCritical(ex, $"Failed to find collection in CosmosDB {_settings.DatabaseName}:{this.CollectionName}");
                return false;
            }

        }

        private async Task<IList<string>> GetContainerNames()
        {
            var containerNames = new List<string>();
            var database = this.Client.GetDatabase(_settings.DatabaseName);
            using var iter = database.GetContainerQueryIterator<ContainerProperties>();
            while (iter.HasMoreResults)
            {
                var response = await iter.ReadNextAsync().ConfigureAwait(false);

                containerNames.AddRange(response.Select(c => c.Id));
            }

            return containerNames;
        }
        
        Container GetContainer(CosmosClient client)
        {
            try
            {
                var container = client.GetContainer(_settings.DatabaseName, this.CollectionName);

                return container;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to connect to CosmosDB {_settings.DatabaseName}:{this.CollectionName}");
                throw;
            }
        }

        /// <summary>
        /// Compute the partition key based on input id
        /// 
        /// For this sample, the partitionkey is the id mod 10
        /// 
        /// In a full implementation, you would update the logic to determine the partition key
        /// </summary>
        /// <param name="id">document id</param>
        /// <returns>the partition key</returns>
        //public static string GetPartitionKey(string id)
        //{
        //    // validate id
        //    if (!string.IsNullOrEmpty(id) &&
        //        id.Length > 5 &&
        //        (id.StartsWith("tt", StringComparison.OrdinalIgnoreCase) || id.StartsWith("nm", StringComparison.OrdinalIgnoreCase)) &&
        //        int.TryParse(id.Substring(2), out int idInt))
        //    {
        //        return (idInt % 10).ToString(CultureInfo.InvariantCulture);
        //    }

        //    throw new ArgumentException("Invalid Partition Key");
        //}

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="T">POCO type to which results are serialized and returned.</typeparam>
        /// <param name="queryDefinition">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        private async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery(QueryDefinition queryDefinition)
        {
            // run query
            var query = this.Container.GetItemQueryIterator<TEntity>(queryDefinition, requestOptions: _options.QueryRequestOptions);

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
        /// <param name="sql">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        private async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery(string sql)
        {
            // run query
            var query = this.Container.GetItemQueryIterator<TEntity>(sql, requestOptions: _options.QueryRequestOptions);

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

        public async Task<TEntity> GetByIdAsync(string id)
        {
            var response = await this.Container.ReadItemAsync<TEntity>(id, ResolvePartitionKey(id)).ConfigureAwait(false);
            return response;
        }

        public async Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument)
        {
            //PartitionKey pk = String.IsNullOrWhiteSpace(partitionKey) ? default : new PartitionKey(partitionKey);
            return await this.Container.ReplaceItemAsync<TEntity>(newDocument, id, ResolvePartitionKey(id)).ConfigureAwait(false);
        }

        public async Task<TEntity> CreateDocumentAsync(TEntity newDocument, PartitionKey partitionKey)
        {
            return await this.Container.CreateItemAsync<TEntity>(newDocument, partitionKey).ConfigureAwait(false);
        }

       
        public async Task<TEntity> UpsertDocumentAsync(TEntity newDocument, PartitionKey partitionKey)
        {
            return await this.Container.UpsertItemAsync<TEntity>(newDocument, partitionKey).ConfigureAwait(false);
        }

        public async Task<bool> DoesExistsAsync(string id)
        {
            //TODO: PartitionKey should be created from a column name not from a value specially ID value ???
            using ResponseMessage response = await this.Container.ReadItemStreamAsync(id, ResolvePartitionKey(id)).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<TEntity> DeleteDocumentAsync(string id, PartitionKey partitionKey)
        {
            return await this.Container.DeleteItemAsync<TEntity>(id, partitionKey).ConfigureAwait(false);
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

            string offsetLimit = string.Format(CultureInfo.InvariantCulture, pagedOffsetString, offset, limit);

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

        public async Task<IEnumerable<TEntity>> GetAllAsync(TypeFilter filter = TypeFilter.any)
        {
            string sql = "select * from m";
            if (filter != TypeFilter.any)
            {
                sql += ($" m.objectType='{0}'", Enum.GetName(typeof(TypeFilter), filter));
            }

            return await InternalCosmosDBSqlQuery(sql).ConfigureAwait(false);
        }

        #region IDisposable
        public void Dispose()
        {
            //_client?.Dispose();
        }
        #endregion
    }
}
