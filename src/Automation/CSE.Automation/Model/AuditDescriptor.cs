// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Automation.Model
{
    internal class AuditDescriptor
    {
        public string CorrelationId { get; set; }
        public string ObjectId { get; set; }
        public string DisplayName { get; set; }
        public string AppId { get; set; }
    }
}
