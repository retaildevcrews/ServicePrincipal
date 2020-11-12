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

        public async Task PutFail(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.Activity.Id.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Fail,
                Code = (int)code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {objectId}");
        }

        public async Task PutPass(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.Activity.Id.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Pass,
                Code = (int)code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime.Value,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {objectId}");
        }

        public async Task PutIgnore(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.Activity.Id.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Ignore,
                Code = (int)code,
                Reason = string.Format(formatCulture, code.Description(), attributeName),
                Message = message,
                Timestamp = auditTime.Value,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                AuditYearMonth = auditTime.Value.ToString("yyyyMM", formatCulture),
            };

            auditRepository.GenerateId(entry);
            await auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            logger.LogTrace($"Logged Audit {code} for {objectId}");
        }

        public async Task PutChange(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.Activity.Id.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Change,
                Code = (int)code,
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

            logger.LogTrace($"Logged Audit {code} for {objectId}");
        }
    }
}
