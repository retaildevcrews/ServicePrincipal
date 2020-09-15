using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSE.Automation.Utilities
{
    public interface IGraphHelper
    {
        Task<IEnumerable<ServicePrincipal>> SeedServicePrincipalDeltaAsync(string selectSPFields);
        Task<IEnumerable<ServicePrincipal>> GetServicePrincipalsByDeltaAsync(string deltaLink);
    }
}