using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.DataAccess
{
    internal interface IActivityHistoryRepository : ICosmosDBRepository<ActivityHistory>
    {
        Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId);
    }
}
