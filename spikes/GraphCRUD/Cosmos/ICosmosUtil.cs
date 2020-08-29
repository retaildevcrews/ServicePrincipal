using Microsoft.Graph;
using System.Threading.Tasks;

namespace GraphCrud
{
    public interface ICosmosUtil
    {
        Task AddServicePrincipalToContainerAsync(ServicePrincipal servicePrincipal);
    }
}