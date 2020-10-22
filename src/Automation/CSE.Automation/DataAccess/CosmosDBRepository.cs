using CSE.Automation.Config;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.DataAccess
{

    class CosmosDBSettings : SettingsBase, ICosmosDBSettings
    {
        private string _uri;
        private string _key;
        private string _databaseName;

        public CosmosDBSettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.CosmosDBURLName)]
        public string Uri
        {
            get { return _uri ?? base.GetSecret(); }
            set { _uri = value; }
        }

        [Secret(Constants.CosmosDBKeyName)]
        public string Key
        {
            get { return _key ?? base.GetSecret(); }
            set { _key = value; }
        }

        [Secret(Constants.CosmosDBDatabaseName)]
        public string DatabaseName
        {
            get { return _databaseName ?? base.GetSecret(); }
            set { _databaseName = value; }
        }


        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Uri)) throw new ConfigurationErrorsException($"{this.GetType().Name}: Uri is invalid");
            if (string.IsNullOrWhiteSpace(this.Key)) throw new ConfigurationErrorsException($"{this.GetType().Name}: Key is invalid");
            if (string.IsNullOrWhiteSpace(this.DatabaseName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: DatabaseName is invalid");
        }
    }

    abstract class CosmosDBRepository<TEntity> : ICosmosDBRepository<TEntity>, IDisposable where TEntity : class
    {
        const string pagedOffsetString = " offset {0} limit {1}";


        private readonly CosmosConfig _options;
        private static CosmosClient _client;
        private Container _container;
        private ContainerProperties _containerProperties;
        private readonly ICosmosDBSettings _settings;
        private readonly ILogger _logger;
        private PropertyInfo _partitionKeyPI;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        protected CosmosDBRepository(ICosmosDBSettings settings, ILogger logger)
        {
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
        private object lockObj = new object();

        // NOTE: CosmosDB library currently wraps the Newtonsoft JSON serializer.  Align Attributes and Converters to Newtonsoft on the domain models.
        CosmosClient Client => _client ??= new CosmosClientBuilder(_settings.Uri, _settings.Key)
                                                    .WithRequestTimeout(TimeSpan.FromSeconds(CosmosTimeout))
                                                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(CosmosTimeout), CosmosMaxRetries)
                                                    .WithSerializerOptions(new CosmosSerializationOptions
                                                    {
                                                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                                                        Indented = false,
                                                        IgnoreNullValues = true
                                                    })
                                                    .Build();
        private Container Container { get { lock (lockObj) { return _container ??= GetContainer(Client); } } }


        public abstract string GenerateId(TEntity entity);


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

        public virtual PartitionKey ResolvePartitionKey(TEntity entity)
        {
            try
            {
                var value = new PartitionKey(_partitionKeyPI.GetValue(entity).ToString());
                return value;
            }
            catch (Exception ex)
            {
                ex.Data["partitionKeyPath"] = _containerProperties.PartitionKeyPath;
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
                _logger.LogDebug($"Test {this.Id} -- '{containerNames}'");
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
                _logger.LogError(ex, $"Failed to find collection in CosmosDB {_settings.DatabaseName}:{this.CollectionName}");
                return false;
            }

        }

        public string Id => $"{DatabaseName}:{CollectionName}";

        /// <summary>
        /// Query the database for all the containers defined and return a list of the container names.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get a proxy to the container.
        /// </summary>
        /// <param name="client">An instance of <see cref="CosmosClient"/></param>
        /// <returns>An instance of <see cref="Container"/>.</returns>
        Container GetContainer(CosmosClient client)
        {
            try
            {
                var container = client.GetContainer(_settings.DatabaseName, this.CollectionName);

                _containerProperties = GetContainerProperties(container).Result;
                var partitionKeyName = _containerProperties.PartitionKeyPath.TrimStart('/');
                _partitionKeyPI = typeof(TEntity).GetProperty(partitionKeyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (_partitionKeyPI is null)
                {
                    throw new ApplicationException($"Failed to find partition key property {partitionKeyName} on {typeof(TEntity).Name}.  Collection definition does not match Entity definition");
                }

                _logger.LogDebug($"{CollectionName} partition key path {_containerProperties.PartitionKeyPath}");

                return container;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to connect to CosmosDB {_settings.DatabaseName}:{this.CollectionName}");
                throw;
            }
        }

        /// <summary>
        /// Get the properties for the container.
        /// </summary>
        /// <returns>An instance of <see cref="ContainerProperties"/> or null.</returns>
        protected async Task<ContainerProperties> GetContainerProperties(Container container = null)
        {
            return (await (container ?? Container).ReadContainerAsync().ConfigureAwait(false)).Resource;

            //var query = new QueryDefinition("select * from c where c.id = @id").WithParameter("@id", this.CollectionName);
            //using var iter = this.Container.Database.GetContainerQueryIterator<ContainerProperties>(query);
            //while (iter.HasMoreResults)
            //{
            //    var response = await iter.ReadNextAsync().ConfigureAwait(false);

            //    return response.First();
            //}
            //return null;
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
        private async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery(string sql, QueryRequestOptions options = null)
        {
            // run query
            var query = this.Container.GetItemQueryIterator<TEntity>(sql, requestOptions: options ?? _options.QueryRequestOptions);

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

        public async Task<TEntity> GetByIdAsync(string id, string partitionKey)
        {
            TEntity entity = null;
            try
            {
                var result = await this.Container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(true);

                entity = result.Resource;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // swallow exception
            }
            return entity;
        }

        public async Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument)
        {
            return await this.Container.ReplaceItemAsync<TEntity>(newDocument, id, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        public async Task<TEntity> CreateDocumentAsync(TEntity newDocument)
        {
            return await this.Container.CreateItemAsync<TEntity>(newDocument, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        public async Task<TEntity> UpsertDocumentAsync(TEntity newDocument)
        {
            // TEST CODE
            //var container = this.Container;
            //var partitionKey = ResolvePartitionKey(newDocument);
            //using (var stream = new StreamReader(this.Client.ClientOptions.Serializer.ToStream(newDocument)))
            //{
            //    var objString = stream.ReadToEnd();
            //    _logger.LogDebug(objString);

            //}
            //return await container.UpsertItemAsync<TEntity>(newDocument, partitionKey).ConfigureAwait(false);
            return await this.Container.UpsertItemAsync<TEntity>(newDocument, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        //public async Task<bool> DoesExistsAsync(string id)
        //{
        //    //TODO: PartitionKey should be created from a column name not from a value specially ID value ???
        //    using ResponseMessage response = await this.Container.ReadItemStreamAsync(id, ResolvePartitionKey(id)).ConfigureAwait(false);
        //    return response.IsSuccessStatusCode;
        //}

        public async Task<TEntity> DeleteDocumentAsync(string id, string partitionKey)
        {
            var query = new QueryDefinition("delete from c where c.id = @id").WithParameter("@id", id);

            var result = await InternalCosmosDBSqlQuery(query).ConfigureAwait(false);

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
