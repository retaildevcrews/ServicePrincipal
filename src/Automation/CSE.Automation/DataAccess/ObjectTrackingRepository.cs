using System;
using System.Configuration;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.DataAccess
{
    internal class ObjectTrackingRepositorySettings : CosmosDBSettings
    {
        public ObjectTrackingRepositorySettings(ISecretClient secretClient) : base(secretClient)
        {
        }

        public string CollectionName { get; set; }
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(CollectionName))
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: CollectionName is invalid");
            }
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
