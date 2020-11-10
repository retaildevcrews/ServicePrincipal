using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IClassifier<TEntity>
        where TEntity : class
    {
        Task<TEntity> Classify(TEntity entity);
    }

    internal interface IServicePrincipalClassifier : IClassifier<ServicePrincipalClassification>
    {
    }
}
