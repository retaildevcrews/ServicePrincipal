﻿using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.Config;
using CSE.Automation.Interfaces;

namespace CSE.Automation.DataAccess
{
    public partial class DAL : IDAL
    {
        const string pagedOffsetString = " offset {0} limit {1}";

        public int DefaultPageSize { get; set; } = 100;
        public int MaxPageSize { get; set; } = 1000;
        public int CosmosTimeout { get; set; } = 60;
        public int CosmosMaxRetries { get; set; } = 10;

        private CosmosConfig cosmosDetails;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="cosmosUrl">CosmosDB Url</param>
        /// <param name="cosmosKey">CosmosDB connection key</param>
        /// <param name="cosmosDatabase">CosmosDB Database</param>
        /// <param name="cosmosCollection">CosmosDB Collection</param>
        public DAL(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            if (cosmosUrl == null)
            {
                throw new ArgumentNullException(nameof(cosmosUrl));
            }

            cosmosDetails = new CosmosConfig
            {
                MaxRows = MaxPageSize,


                Timeout = CosmosTimeout,
                CosmosCollection = cosmosCollection,
                CosmosDatabase = cosmosDatabase,
                CosmosKey = cosmosKey,
                CosmosUrl = cosmosUrl.AbsoluteUri
            };

            // create the CosmosDB client and container
            cosmosDetails.Client = OpenAndTestCosmosClient(cosmosUrl, cosmosKey, cosmosDatabase, cosmosCollection);
            cosmosDetails.Container = cosmosDetails.Client.GetContainer(cosmosDatabase, cosmosCollection);
        }

        /// <summary>
        /// Recreate the Cosmos Client / Container (after a key rotation)
        /// </summary>
        /// <param name="cosmosUrl">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <param name="force">force reconnection even if no params changed</param>
        /// <returns>Task</returns>
        public async Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false)
        {
            await Task.Run(() =>
                {
                    if (cosmosUrl == null)
                    {
                        throw new ArgumentNullException(nameof(cosmosUrl));
                    }

                    if (force ||
                        cosmosDetails.CosmosCollection != cosmosCollection ||
                        cosmosDetails.CosmosDatabase != cosmosDatabase ||
                        cosmosDetails.CosmosKey != cosmosKey ||
                        cosmosDetails.CosmosUrl != cosmosUrl.AbsoluteUri)
                    {
                        CosmosConfig d = new CosmosConfig
                        {
                            CosmosCollection = cosmosCollection,
                            CosmosDatabase = cosmosDatabase,
                            CosmosKey = cosmosKey,
                            CosmosUrl = cosmosUrl.AbsoluteUri
                        };

                        // open and test a new client / container
                        d.Client = OpenAndTestCosmosClient(cosmosUrl, cosmosKey, cosmosDatabase, cosmosCollection);
                        d.Container = d.Client.GetContainer(cosmosDatabase, cosmosCollection);

                        // set the current CosmosDetail
                        cosmosDetails = d;
                    }
                }).ConfigureAwait(false);
        }

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
            var c = new CosmosClient(cosmosUrl.AbsoluteUri, cosmosKey, cosmosDetails.CosmosClientOptions);
#pragma warning restore CA2000 // Dispose objects before losing scope
            var con = c.GetContainer(cosmosDatabase, cosmosCollection);

            //TODO: commenting out for the moment.  Need a good way to test that doesn't require a document
            //await con.ReadItemAsync<dynamic>("action", new PartitionKey("0")).ConfigureAwait(false);

            return c;
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
        public static string GetPartitionKey(string id)
        {
            // validate id
            if (!string.IsNullOrEmpty(id) &&
                id.Length > 5 &&
                (id.StartsWith("tt", StringComparison.OrdinalIgnoreCase) || id.StartsWith("nm", StringComparison.OrdinalIgnoreCase)) &&
                int.TryParse(id.Substring(2), out int idInt))
            {
                return (idInt % 10).ToString(CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Invalid Partition Key");
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="T">POCO type to which results are serialized and returned.</typeparam>
        /// <param name="queryDefinition">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        private async Task<IEnumerable<T>> InternalCosmosDBSqlQuery<T>(QueryDefinition queryDefinition)
        {
            // run query
            var query = cosmosDetails.Container.GetItemQueryIterator<T>(queryDefinition, requestOptions: cosmosDetails.QueryRequestOptions);

            List<T> results = new List<T>();

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
        private async Task<IEnumerable<T>> InternalCosmosDBSqlQuery<T>(string sql)
        {
            // run query
            var query = cosmosDetails.Container.GetItemQueryIterator<T>(sql, requestOptions: cosmosDetails.QueryRequestOptions);

            List<T> results = new List<T>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }
            return results;
        }

        public async Task<T> GetById<T>(string Id)
        {
            var response = await cosmosDetails.Container.ReadItemAsync<T>(Id, new PartitionKey(GetPartitionKey(Id))).ConfigureAwait(false);
            return response;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Using lower case with cosmos queries as tested.")]
        public async Task<IEnumerable<T>> GetPagedAsync<T>(string q, int offset = 0, int limit = Constants.DefaultPageSize)
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

            return await InternalCosmosDBSqlQuery<T>(queryDefinition).ConfigureAwait(false);

        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(TypeFilter filter = TypeFilter.any)
        {
            string sql = "select * from m";
            if (filter != TypeFilter.any)
            {
                sql += ($" m.objectType='{0}'", Enum.GetName(typeof(TypeFilter), filter));
            }

            return await InternalCosmosDBSqlQuery<T>(sql).ConfigureAwait(false);
        }
    }
}