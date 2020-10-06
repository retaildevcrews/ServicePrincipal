using CSE.Automation.Interfaces;
using CSE.Automation.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security;
using System.Text;
using CSE.Automation.Model;

namespace CSE.Automation.DataAccess
{
   
    class DALResolver : IServiceResolver
    {
        private ConcurrentDictionary<string, IDAL> _registeredDALs = new System.Collections.Concurrent.ConcurrentDictionary<string, IDAL>();
        private readonly CosmosDBSettings _settings;

        public DALResolver (CosmosDBSettings settings)
        {
            _settings = settings;
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

            return new DAL(new Uri(_settings.Uri), _settings.Key, _settings.DatabaseName, _settings.CollectionName);


        }

        //public IDAL GetDAL(DALCollection collection)
        //{
        //    string collectionName = Enum.GetName(typeof(DALCollection), collection);
        //    return _registeredDALs.GetOrAdd(collectionName, CreateDAL(collection));

        //}

        public T GetService<T>(string keyName)
        {
            var targetInterface = typeof(IDAL);
            if (typeof(T) != targetInterface)
                throw new InvalidCastException($"For DAL resolver type T must be of type {targetInterface.Name}");

            DALCollection collectionName = Enum.Parse<DALCollection>(keyName);
            
            return (T) _registeredDALs.GetOrAdd(keyName, CreateDAL(collectionName)) ;
        }


    }
}
