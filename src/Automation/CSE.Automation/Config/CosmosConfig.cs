// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace CSE.Automation.Config
{
    /// <summary>
    /// Internal class for Cosmos config
    /// </summary>
    internal class CosmosConfig
    {
        //public CosmosClient Client;
        //public Container Container;

        // default values for Cosmos Options
        public int MaxRows = 1000;
        public int Timeout = 60;
        public int Retries = 10;

        // Cosmos connection fields
        //public string CosmosUrl;
        //public string CosmosKey;
        //public string CosmosDatabase;
        //public string CosmosCollection;

        // member variables
        private QueryRequestOptions queryRequestOptions;
        private CosmosClientOptions cosmosClientOptions;

        // CosmosDB query request options
        public QueryRequestOptions QueryRequestOptions
        {
            get
            {
                if (queryRequestOptions == default)
                {
                    queryRequestOptions = new QueryRequestOptions { MaxItemCount = MaxRows };//, ConsistencyLevel = ConsistencyLevel.Session };
                }

                return queryRequestOptions;
            }
        }

        // default protocol is tcp, default connection mode is direct
        public CosmosClientOptions CosmosClientOptions
        {
            get
            {
                if (cosmosClientOptions == default)
                {
                    cosmosClientOptions = new CosmosClientOptions { RequestTimeout = TimeSpan.FromSeconds(Timeout), MaxRetryAttemptsOnRateLimitedRequests = Retries, MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(Timeout) };
                }

                return cosmosClientOptions;
            }
        }
    }
}
