using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IActivityHistoryRepository : ICosmosDBRepository<ActivityHistory>
    {
        Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId);
    }
}
