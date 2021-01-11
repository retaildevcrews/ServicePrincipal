// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.Services
{
    internal class AuditService : IAuditService
    {
        private readonly IAuditRepository auditRepository;
        private readonly ILogger logger;

        public AuditService(IAuditRepository auditRepository, ILogger<AuditService> logger)
        {
            this.auditRepository = auditRepository;
            this.logger = logger;
        }

        public async Task PutFail(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                Descriptor = descriptor,
                Type = AuditActionType.Fail,
                Code = code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {descriptor.ObjectId}");
        }

        public async Task PutPass(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                Descriptor = descriptor,
                Type = AuditActionType.Pass,
                Code = code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime.Value,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {descriptor.ObjectId}");
        }

        public async Task PutIgnore(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                Descriptor = descriptor,
                Type = AuditActionType.Ignore,
                Code = code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime.Value,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {descriptor.ObjectId}");
        }

        public async Task PutChange(AuditDescriptor descriptor, AuditCode code, string attributeName, string existingAttributeValue, string updatedAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                Descriptor = descriptor,
                Type = AuditActionType.Change,
                Code = code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime.Value,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                UpdatedAttributeValue = updatedAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {descriptor.ObjectId}");
        }
    }
}
