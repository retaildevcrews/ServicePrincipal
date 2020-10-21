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
        }

        public ProcessorConfiguration Get(string id, byte[] defaultConfig)
        {
            if (!_configRepository.DoesExistsAsync(id).Result)
            {

                if (defaultConfig == null || defaultConfig.Length == 0)
                    throw new NullReferenceException("Null or empty initial Configuration Document resource.");
                var initalDocumentAsString = System.Text.Encoding.Default.GetString(defaultConfig);

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
