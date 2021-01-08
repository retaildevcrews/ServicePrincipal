// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.OData.UriParser;

namespace CSE.Automation.Interfaces
{
    internal interface IAuditService
    {
        Task PutFail(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null);

        Task PutPass(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null);

        Task PutIgnore(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null);

        Task PutChange(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string updatedAttributeValue, string message = null, DateTimeOffset? auditTime = null);
    }
}
