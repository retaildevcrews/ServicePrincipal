using System;
using CSE.Automation.Processors;

namespace CSE.Automation.Tests.Mocks
{
    internal class ServicePrincipalProcessorSettingsMock : IServicePrincipalProcessorSettings
    {
        public string QueueConnectionString { get; set; } = "default";
        public string EvaluateQueueName { get; set; } = "evaluate";
        public string UpdateQueueName { get; set; } = "update";
        public string DiscoverQueueName { get; set; } = "discover";
        public UpdateMode AADUpdateMode { get; set; } = UpdateMode.ReportOnly;
        public Guid ConfigurationId { get; set; } = Guid.Empty;
        public int VisibilityDelayGapSeconds { get; set; } = 0;
        public int QueueRecordProcessThreshold { get; set; } = 0;
        public void Validate()
        {
        }
    }
}
