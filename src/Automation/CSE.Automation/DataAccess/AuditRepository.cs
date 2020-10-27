using System;
using System.Configuration;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.DataAccess
{
    internal class AuditRespositorySettings : CosmosDBSettings
    {
        public AuditRespositorySettings(ISecretClient secretClient) : base(secretClient) { }

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
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            return entity.Id;
        }

        public override string CollectionName => _settings.CollectionName;
    }
}
