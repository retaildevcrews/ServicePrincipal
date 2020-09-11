using System;

namespace CSE.Automation.Model
{
    class ProcessorAuditEntry : GraphModel
    {
        //TODO: Do I need Id in this class as well?
        public DateTime LastRunTime { get; set; }

        public int ObjectsProcessedCount { get; set; }

        //TODO: is this the same as status in the base class or does it have different values?
        public Status FinalStatus { get; set; }
    }
}
