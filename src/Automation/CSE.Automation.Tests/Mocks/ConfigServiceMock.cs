using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class ConfigServiceMock : IConfigService<ProcessorConfiguration>
    {
        public ProcessorConfiguration Config { get; set; }

        public async Task<ProcessorConfiguration> Put(ProcessorConfiguration newDocument)
        {
            Config = newDocument;
            return await Task.FromResult(Config);
        }

        public ProcessorConfiguration Get(string id, ProcessorType processorType, string defaultConfigResourceName,
            bool createIfNotFound = false)
        {
            return Config;
        }

        public async Task Lock(string configId, string lockingActivityID, string defaultConfigResourceName)
        {
            Config.LockingActivityId = lockingActivityID;
            Config.IsProcessorLocked = true;
            await Task.CompletedTask;
        }

        public async Task Unlock(string configId)
        {
            Config.LockingActivityId = null;
            Config.IsProcessorLocked = false;
            await Task.CompletedTask;
        }
    }
}
