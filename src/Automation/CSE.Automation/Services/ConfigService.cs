// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.DataAccess;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
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
        private readonly IConfigRepository configRepository;
        private readonly ILogger configLogger;
        public ConfigService(IConfigRepository configRepository, ILogger<ConfigService> configLogger)
        {
            this.configRepository = configRepository;
            this.configLogger = configLogger;
        }

        public ProcessorConfiguration Get(string id, ProcessorType processorType, string defaultConfigResourceName, bool createIfNotFound = true)
        {
            ProcessorConfiguration configuration = configRepository.GetByIdAsync(id, processorType.ToString()).GetAwaiter().GetResult();
            if (configuration == null && createIfNotFound)
            {
                if (string.IsNullOrWhiteSpace(defaultConfigResourceName))
                {
                    throw new NullReferenceException("Null or empty initial Configuration Document resource name.");
                }

                byte[] defaultConfig = (byte[])Resources.ResourceManager.GetObject(defaultConfigResourceName, Resources.Culture);
                var initialDocumentAsString = System.Text.Encoding.Default.GetString(defaultConfig);

                try
                {
                    ProcessorConfiguration defaultConfiguration = JsonConvert.DeserializeObject<ProcessorConfiguration>(initialDocumentAsString);
                    defaultConfiguration.Id = id;
                    return configRepository.CreateDocumentAsync(defaultConfiguration).Result;
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
            return await configRepository.ReplaceDocumentAsync(newDocument.Id, newDocument).ConfigureAwait(false);
        }

        public async Task Lock(string configId, string lockingActivityId, string defaultConfigResourceName)
        {
            try
            {
                Get(configId, ProcessorType.ServicePrincipal, defaultConfigResourceName);
                var configWithMeta = await configRepository.GetByIdWithMetaAsync(configId, "ServicePrincipal").ConfigureAwait(false);
                ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = configWithMeta.ETag };
                ProcessorConfiguration config = configWithMeta.Resource;
                if (config.IsProcessorLocked)
                {
                    throw new AccessViolationException("Processor Already Locked By Another Process");
                }
                else
                {
                    config.IsProcessorLocked = true;
                    config.LockingActivityId = lockingActivityId;
                    var id = configRepository.ReplaceDocumentAsync(config.Id, config, requestOptions).Result.Id;
                    configLogger.LogInformation($"Acquired lock for activity: {id}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lock Unsuccessful", ex);
            }
        }

        public async Task Unlock(string configId)
        {
            configId = string.IsNullOrEmpty(configId) ? "02a54ac9-441e-43f1-88ee-fde420db2559" : configId;
            var config = configRepository.GetByIdAsync(configId, "ServicePrincipal").Result;
            config.IsProcessorLocked = false;
            config.LockingActivityId = null;
            await configRepository.ReplaceDocumentAsync(config.Id, config).ConfigureAwait(false);
        }
    }
}
