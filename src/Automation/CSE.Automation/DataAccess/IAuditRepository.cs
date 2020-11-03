using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.DataAccess
{
    internal interface IAuditRepository : ICosmosDBRepository<AuditEntry> { }
}
