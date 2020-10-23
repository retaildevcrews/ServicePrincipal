using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

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

        public async Task PutFail(string attributeName, string existingAttributeValue, string reason,
            ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry();
            context ??= new ActivityContext();
            entry.CorrelationId = objectId ?? context.ActivityId.ToString();
            entry.Type = AuditActionType.Fail;
            entry.Reason = reason;
            entry.Timestamp = auditTime ?? entry.Timestamp;
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            entry.AttributeName = attributeName;
            entry.ExistingAttributeValue = existingAttributeValue;
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutPass(string attributeName, string existingAttributeValue, string reason,
            ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry();
            context ??= new ActivityContext();
            entry.CorrelationId = objectId ?? context.ActivityId.ToString();
            entry.Type = AuditActionType.Pass;
            entry.Reason = reason;
            entry.Timestamp = auditTime ?? entry.Timestamp;
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            entry.AttributeName = attributeName;
            entry.ExistingAttributeValue = existingAttributeValue;
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutChange(string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason,
            ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry();
            context ??= new ActivityContext();
            entry.CorrelationId = objectId ?? context.ActivityId.ToString();
            entry.Type = AuditActionType.Change;
            entry.Reason = reason;
            entry.Timestamp = auditTime ?? entry.Timestamp;
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            entry.AttributeName = attributeName;
            entry.ExistingAttributeValue = existingAttributeValue;
            entry.UpdatedAttributeValue = updatedAttributeValue;
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public async Task PutFailThenChange(string attributeName, string existingAttributeValue, string updatedAttributeValue, string reason,
            ActivityContext context = null, string objectId = null, DateTimeOffset? auditTime = null)
        {
            var entry = new AuditEntry();
            context ??= new ActivityContext();
            entry.CorrelationId = objectId ?? context.ActivityId.ToString();
            entry.Type = AuditActionType.Fail;
            entry.Reason = reason;
            entry.Timestamp = auditTime ?? entry.Timestamp;
            entry.AuditYearMonth = entry.Timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture);
            entry.AttributeName = attributeName;
            entry.ExistingAttributeValue = existingAttributeValue;
            entry.UpdatedAttributeValue = updatedAttributeValue;
            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);

            entry.UpdatedAttributeValue = updatedAttributeValue;
            entry.Type = AuditActionType.Fail;

            await _auditRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }
    }
}
