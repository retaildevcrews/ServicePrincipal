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
        private IConfigRepository _configRepository;
        public ConfigService(IConfigRepository configRepository)
        {
            this._configRepository = configRepository;
        }

        public ProcessorConfiguration Get(string id, ProcessorType processorType, byte[] defaultConfig)
        {
            ProcessorConfiguration configuration = _configRepository.GetByIdAsync(id, processorType.ToString()).GetAwaiter().GetResult();
            if (configuration == null)
            {
                if (defaultConfig == null || defaultConfig.Length == 0)
                {
                    throw new NullReferenceException("Null or empty initial Configuration Document resource.");
                }

                var initalDocumentAsString = System.Text.Encoding.Default.GetString(defaultConfig);

                try
                {
                    ProcessorConfiguration defaultConfiguration = JsonConvert.DeserializeObject<ProcessorConfiguration>(initalDocumentAsString);
                    return _configRepository.CreateDocumentAsync(defaultConfiguration).Result;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Unable to deserialize Initial Configuration Document.", ex);
                }
            }

            return configuration;

        }
        public async Task<ProcessorConfiguration> Put(ProcessorConfiguration newDocument)
        {
            return await _configRepository.ReplaceDocumentAsync(newDocument.Id, newDocument).ConfigureAwait(false);
        }
    }
}
