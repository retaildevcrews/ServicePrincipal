using CSE.Automation.DataAccess;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        public ProcessorConfiguration Get(string id)
        {
            if (!_configRepository.DoesExistsAsync(id).Result)
            {

                if (Resources.ServicePrincipalProcessorConfiguration == null || Resources.ServicePrincipalProcessorConfiguration.Length == 0)
                    throw new NullReferenceException("Null or empty initial Configuration Document resource.");
                var initalDocumentAsString = System.Text.Encoding.Default.GetString(Resources.ServicePrincipalProcessorConfiguration);

                try
                {
                    ProcessorConfiguration initialConfigDocumentAsJson = JsonConvert.DeserializeObject<ProcessorConfiguration>(initalDocumentAsString);
                    return _configRepository.CreateDocumentAsync(initialConfigDocumentAsJson, new PartitionKey(initialConfigDocumentAsJson.Id)).Result;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Unable to deserialize Initial Configuration Document.", ex);
                }
            }
            else
            {
                return _configRepository.GetByIdAsync(id).Result;
            }
        }
        public async Task<ProcessorConfiguration> Put(ProcessorConfiguration newDocument)
        {
            return await _configRepository.ReplaceDocumentAsync(newDocument.Id, newDocument).ConfigureAwait(false);
        }
    }
}
