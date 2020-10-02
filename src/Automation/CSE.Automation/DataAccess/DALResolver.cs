using CSE.Automation.Interfaces;
using CSE.Automation.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CSE.Automation.DataAccess
{
    public class DALResolver : IServiceResolver
    {
        private ConcurrentDictionary<string, IDAL> _registeredDALs = new System.Collections.Concurrent.ConcurrentDictionary<string, IDAL>();
        private ISecretClient _secretClient;
        private readonly string _cosmosURL;
        private readonly string _cosmosKey;
        private readonly string _cosmosDatabaseName;

        public DALResolver (ISecretClient secretClient)
        {
            _secretClient = secretClient;

            _cosmosURL = _secretClient.GetSecretValue(Constants.CosmosDBURLName);
            _cosmosKey = _secretClient.GetSecretValue(Constants.CosmosDBKeyName);
            _cosmosDatabaseName = _secretClient.GetSecretValue(Constants.CosmosDBDatabaseName);
        }

        private IDAL CreateDAL(DALCollection collectionName)
        {
            string collectionNameKey = default;
            string cosmosCollectionName = default;

            switch (collectionName){
                case DALCollection.Audit:
                    collectionNameKey = Constants.CosmosDBAuditCollectionName;
                    break;
                case DALCollection.ProcessorConfiguration:
                    collectionNameKey = Constants.CosmosDBConfigCollectionName;
                    break;
                case DALCollection.ObjectTracking:
                    collectionNameKey = Constants.CosmosDBOjbectTrackingCollectionName;
                    break;
            }

            cosmosCollectionName = _secretClient.GetSecretValue(collectionNameKey);

            return new DAL(new Uri(_cosmosURL), _cosmosKey, _cosmosDatabaseName, cosmosCollectionName);


        }

        //public IDAL GetDAL(DALCollection collection)
        //{
        //    string collectionName = Enum.GetName(typeof(DALCollection), collection);
        //    return _registeredDALs.GetOrAdd(collectionName, CreateDAL(collection));

        //}

        public T GetService<T>(string keyName)
        {

            if (typeof(T) != typeof(IDAL))
                throw new InvalidCastException("For DAL resolver type T must be of type IDAL");

            DALCollection collectionName = Enum.Parse<DALCollection>(keyName);
            
            return (T) _registeredDALs.GetOrAdd(keyName, CreateDAL(collectionName)) ;
        }


    }
}
