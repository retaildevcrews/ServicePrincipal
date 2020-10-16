using CSE.Automation.DataAccess;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Services
{
    internal class ConfigService : IConfigService<ProcessorConfiguration>
    {
        private ConfigRepository _configRepository;

        public ConfigService(ConfigRepository configRepository)
        {
            this._configRepository = configRepository;
            if (this._configRepository.Test().Result == false)
            {
                throw new ApplicationException($"Repository {_configRepository.DatabaseName}:{_configRepository.CollectionName} failed connection test");
            }

        }

        public async Task<ProcessorConfiguration> GetByIdAsync(string id)
        {
            var response = await this._configRepository.GetByIdAsync(id).ConfigureAwait(false);
            return response;
        }

        public async Task<ProcessorConfiguration> ReplaceDocumentAsync(string id, ProcessorConfiguration newDocument)
        {
            return await this._configRepository.ReplaceDocumentAsync(id, newDocument).ConfigureAwait(false);
        }

        public async Task<ProcessorConfiguration> CreateDocumentAsync(ProcessorConfiguration newDocument, PartitionKey partitionKey)
        {
            return await this._configRepository.CreateDocumentAsync(newDocument, partitionKey).ConfigureAwait(false);
        }

        public async Task<ProcessorConfiguration> UpsertDocumentAsync(ProcessorConfiguration newDocument, PartitionKey partitionKey)
        {
            return await this._configRepository.UpsertDocumentAsync(newDocument, partitionKey).ConfigureAwait(false);
        }

        public async Task<bool> DoesExistsAsync(string id)
        {
            return await this._configRepository.DoesExistsAsync(id).ConfigureAwait(false);
        }

        public async Task<ProcessorConfiguration> DeleteDocumentAsync(string id, PartitionKey partitionKey)
        {
            return await this._configRepository.DeleteDocumentAsync(id, partitionKey).ConfigureAwait(false);
        }
    }
}
