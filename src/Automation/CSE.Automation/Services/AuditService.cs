using System;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

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

        public async Task PutFail(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string reason, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Fail,
                Reason = reason,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue

            };
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutPass(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string reason, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Pass,
                Reason = reason,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue

            };
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutIgnore(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string reason, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Ignore,
                Reason = reason,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue

            };
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutChange(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Change,
                Reason = reason,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                UpdatedAttributeValue = updatedAttributeValue,
            };
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutFailThenChange(ActivityContext context, string objectId, string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry
            {
                CorrelationId = context.ActivityId.ToString(),
                ObjectId = objectId,
                Type = AuditActionType.Fail,
                Reason = reason,
                Timestamp = auditTime ?? DateTimeOffset.Now,
                AttributeName = attributeName,
                ExistingAttributeValue = existingAttributeValue,
                UpdatedAttributeValue = updatedAttributeValue,

            };
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);

            // reassign entry to ensure fail gets written before change
            entry = await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            entry.UpdatedAttributeValue = updatedAttributeValue;
            entry.Type = AuditActionType.Change;

            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }
    }
}
