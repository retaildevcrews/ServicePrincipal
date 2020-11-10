using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IAuditRepository : ICosmosDBRepository<AuditEntry> { }
}
