using System.Configuration;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.DataAccess
{
    internal class ConfigRespositorySettings : CosmosDBSettings
    {
        public ConfigRespositorySettings(ISecretClient secretClient) : base(secretClient)
        {
        }

        public string CollectionName { get; set; }

        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrWhiteSpace(CollectionName))
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: CollectionName is invalid");
            }
        }
    }

    internal interface IConfigRepository : ICosmosDBRepository<ProcessorConfiguration> { }

    internal class ConfigRepository : CosmosDBRepository<ProcessorConfiguration>, IConfigRepository
    {
        private readonly ConfigRespositorySettings _settings;
        public ConfigRepository(ConfigRespositorySettings settings, ILogger<ConfigRepository> logger) : base(settings, logger)
        {
            _settings = settings;
        }

        public override string GenerateId(ProcessorConfiguration entity)
        {
            return entity.Id;
        }

        public override string CollectionName => _settings.CollectionName;
    }
}
