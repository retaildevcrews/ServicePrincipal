﻿using System;
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
    internal class ConfigRespositorySettings : CosmosDBSettings
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

        public override PartitionKey ResolvePartitionKey(string entityId)
        {
            return new PartitionKey($"ServicePrincipal");
        }

        public override string CollectionName => _settings.CollectionName;
    }
}