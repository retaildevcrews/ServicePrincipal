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
    internal class AuditRespositorySettings : CosmosDBSettings
    {
        private string _collectionName;

        public AuditRespositorySettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.CosmosDBAuditCollectionName)]
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

    internal interface IAuditRepository : ICosmosDBRepository<AuditEntry> { }
    internal class AuditRepository : CosmosDBRepository<AuditEntry>, IAuditRepository
    {
        private readonly AuditRespositorySettings _settings;
        public AuditRepository(AuditRespositorySettings settings, ILogger<AuditRepository> logger) : base(settings, logger)
        {
            _settings = settings;
        }

        public override string GenerateId(AuditEntry entity)
        {
            if (string.IsNullOrWhiteSpace(entity.CorrelationId))
            {
                entity.CorrelationId = Guid.NewGuid().ToString();
            }
            return entity.CorrelationId;
        }

        public override string CollectionName => _settings.CollectionName;
    }
}
