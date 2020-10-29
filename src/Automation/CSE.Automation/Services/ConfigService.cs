using CSE.Automation.DataAccess;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Services
{
    internal class ConfigService : IConfigService<ProcessorConfiguration>
    {
        private readonly IConfigRepository _configRepository;
        public ConfigService(IConfigRepository configRepository)
        {
            this._configRepository = configRepository;
        }

        public ProcessorConfiguration Get(string id, ProcessorType processorType, string defaultConfigResourceName)
        {
            ProcessorConfiguration configuration = _configRepository.GetByIdAsync(id, processorType.ToString()).GetAwaiter().GetResult();
            if (configuration == null)
            {
                if (string.IsNullOrWhiteSpace(defaultConfigResourceName))
                {
                    throw new NullReferenceException("Null or empty initial Configuration Document resource name.");
                }

                byte[] defaultConfig = (byte[])Resources.ResourceManager.GetObject(defaultConfigResourceName, Resources.Culture);
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

        public async Task Lock()
        {
            try
            {
                Get("02a54ac9-441e-43f1-88ee-fde420db2559", ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration");
                var configWithMeta = _configRepository.GetByIdWithMetaAsync("02a54ac9-441e-43f1-88ee-fde420db2559", "ServicePrincipal").Result;
                ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = configWithMeta.ETag };
                ProcessorConfiguration config = configWithMeta.Resource;
                if (config.IsProcessorLocked)
                {
                    throw new AccessViolationException("Processor Already Locked By Another Process");
                }
                else
                {
                    config.IsProcessorLocked = true;
                    string id = _configRepository.ReplaceDocumentAsync(config.Id, config, requestOptions).Result.Id;
                    Console.WriteLine("Lock Successfull Acquired For: " + id);
                }
            }
            catch (Exception)
            {
                throw new AccessViolationException("Processor Already Locked By Another Process");
            }
        }

        public async Task Unlock()
        {
            var config = _configRepository.GetByIdAsync("02a54ac9-441e-43f1-88ee-fde420db2559", "ServicePrincipal").Result;
            config.IsProcessorLocked = false;
            await _configRepository.ReplaceDocumentAsync(config.Id, config).ConfigureAwait(false);
        }
    }
}
