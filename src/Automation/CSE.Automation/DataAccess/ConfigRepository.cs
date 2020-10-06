using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.DataAccess
{
    class ConfigRespositorySettings : CosmosDBSettings
    {
        public ConfigRespositorySettings(ISecretClient secretClient) : base(secretClient)
        {
        }

        [Secret(Constants.CosmosDBConfigCollectionName)]
        public string CollectionName => base.GetSecret();

        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.CollectionName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: CollectionName is invalid");
        }
    }

    interface IConfigRepository{}
    class ConfigRepository : CosmosDBRepository, IConfigRepository
    {
        private readonly ConfigRespositorySettings _settings;
        public ConfigRepository(ConfigRespositorySettings settings, ILogger<ConfigRepository> logger) : base(settings, logger)
        {
            _settings = settings;
        }

        public override string GenerateId<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string CollectionName  => _settings.CollectionName;
    }
}
