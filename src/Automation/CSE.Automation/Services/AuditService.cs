using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.Services
{


    internal class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger _logger;

        public AuditService(IAuditRepository auditRepository, ILogger<AuditService> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        public async Task PutFail(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
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

            _auditRepository.GenerateId(entry);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            _logger.LogTrace($"Logged Audit {code} for {objectId}");
        }

        public async Task PutPass(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
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

            _auditRepository.GenerateId(entry);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            _logger.LogTrace($"Logged Audit {code} for {objectId}");
        }

        public async Task PutIgnore(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
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

            _auditRepository.GenerateId(entry);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            _logger.LogTrace($"Logged Audit {code} for {objectId}");
        }

        public async Task PutChange(ActivityContext context, AuditCode code, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string message = null, DateTimeOffset? auditTime = null)
        {
            var formatCulture = CultureInfo.CurrentCulture;
            auditTime ??= DateTimeOffset.Now;

            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
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

            _auditRepository.GenerateId(entry);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            _logger.LogTrace($"Logged Audit {code} for {objectId}");
        }
    }
}
