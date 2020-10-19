using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace CSE.Automation.Model
{
    public class AuditEntry
    {
        [JsonProperty(PropertyName = "id")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "actionType")]
        public string ActionType { get; set; }

        [JsonProperty(PropertyName = "actionReason")]
        public string ActionReason { get; set; }

        [JsonProperty(PropertyName = "actionDateTime")]
        public DateTime ActionDateTime { get; set; }

        [JsonProperty(PropertyName = "targetObject")]
        public Object TargetObject { get; set; }
        public AuditEntry()
        {
        }
        public AuditEntry(object originalDocument)
        {
            if (originalDocument is null)
                throw new ArgumentNullException(nameof(originalDocument));

            this.TargetObject = originalDocument;
            this.ActionDateTime = DateTime.UtcNow;
        }
    }
}

