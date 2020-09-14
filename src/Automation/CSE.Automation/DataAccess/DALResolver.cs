using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CSE.Automation.DataAccess
{
    public class DALResolver : IDALResolver
    {
        private ConcurrentDictionary<string, IDAL> _registeredDALs = new System.Collections.Concurrent.ConcurrentDictionary<string, IDAL>();
        private ISecretClient _secretClient;
        private SecureString _cosmosURL;
        private SecureString _cosmosKey;
        private SecureString _cosmosDatabaseName;

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
            SecureString cosmosCollectionName = default;

            switch (collectionName){
                case DALCollection.Audit:
                    collectionNameKey = Constants.CosmosDBAuditCollectionName;
                    break;
                case DALCollection.Configuration:
                    collectionNameKey = Constants.CosmosDBConfigCollectionName;
                    break;
                case DALCollection.ObjectTracking:
                    collectionNameKey = Constants.CosmosDBOjbectTrackingCollectionName;
                    break;
            }

            cosmosCollectionName = _secretClient.GetSecretValue(collectionNameKey);

            return new DAL(new Uri(SecureStringHelper.ConvertToUnsecureString(_cosmosURL)),
                           SecureStringHelper.ConvertToUnsecureString(_cosmosKey),
                           SecureStringHelper.ConvertToUnsecureString(_cosmosDatabaseName),
                           SecureStringHelper.ConvertToUnsecureString(cosmosCollectionName));


        }

        public IDAL GetDAL(DALCollection collection)
        {
            string collectionName = Enum.GetName(typeof(DALCollection), collection);
            return _registeredDALs.GetOrAdd(collectionName, CreateDAL(collection));

        }


    }
}
