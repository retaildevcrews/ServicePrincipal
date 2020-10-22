using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
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

        public async Task<AuditEntry> Put(object originalDocument, string actionType = "", string actionReason = "")
        {
          var entry = new AuditEntry(originalDocument);
          entry.ActionType = actionType;
          entry.ActionReason = actionReason;
          _auditRepository.GenerateId(entry);
          _logger.LogInformation($"{actionType ?? "unknown action"}: {actionReason ?? "unknown reason" }");
          return await _auditRepository.CreateDocumentAsync(entry);
        }
    }
}
