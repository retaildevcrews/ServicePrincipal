// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace CSE.Automation.Model
{
    internal class AuditEntry
    {
        public string Id { get; set; }

        public AuditDescriptor Descriptor { get; set; }

        public AuditActionType Type { get; set; }

        public AuditCode Code { get; set; }

        public string Reason { get; set; }

        public string Message { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string AuditYearMonth { get; set; }

        public string AttributeName { get; set; }

        public string ExistingAttributeValue { get; set; }

        public string UpdatedAttributeValue { get; set; }
    }
}
