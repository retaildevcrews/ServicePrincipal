using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuditActionType
    {
        Pass,
        Fail,
        Change,
        Ignore
    }

    public class AuditEntry
    {
        public string CorrelationId { get; set; }

        public string ObjectId { get; set; }

        public AuditActionType Type { get; set; }

        public string Reason { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string AuditYearMonth { get; set; }

        public string AttributeName { get; set; }

        public string ExistingAttributeValue { get; set; }

        public string UpdatedAttributeValue { get; set; }

    }
}

