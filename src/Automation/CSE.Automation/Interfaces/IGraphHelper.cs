using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSE.Automation.Utilities
{
    public interface IGraphHelper
    {
        void CreateUpdateServicePrincipalNote(string servicePrincipalId, string servicePrincipalNote);
        Task<IEnumerable<ServicePrincipal>> GetAllServicePrincipalsAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<ServicePrincipal>> GetServicePrincipalsDeltaAsync();
        Task<IEnumerable<User>> GetUsersDeltaAsync();
    }
}