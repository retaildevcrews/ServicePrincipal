using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.DataAccess
{
    internal class ObjectTrackingRepositorySettings : CosmosDBSettings
    {
        private string _collectionName;

        public ObjectTrackingRepositorySettings(ISecretClient secretClient) : base(secretClient)
        {
        }

        [Secret(Constants.CosmosDBOjbectTrackingCollectionName)]
        public string CollectionName
        {
            get { return _collectionName ?? base.GetSecret(); }
            set { _collectionName = value; }
        }
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.CollectionName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: CollectionName is invalid");
        }
    }

    internal interface IObjectTrackingRepository : ICosmosDBRepository<TrackingModel> { }
    internal class ObjectTrackingRepository : CosmosDBRepository<TrackingModel>, IObjectTrackingRepository
    {
        private readonly ObjectTrackingRepositorySettings _settings;
        public ObjectTrackingRepository(ObjectTrackingRepositorySettings settings, ILogger<ObjectTrackingRepository> logger) : base(settings, logger)
        {
            _settings = settings;
        }

        public override string GenerateId(TrackingModel entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            return entity.Id;
        }

        public override string CollectionName => _settings.CollectionName;
        
    }
}
