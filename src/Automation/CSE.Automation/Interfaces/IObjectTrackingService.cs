using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IObjectTrackingService
    {
        Task<TrackingModel> Get<TEntity>(string id) where TEntity : GraphModel;
        Task<TEntity> GetAndUnwrap<TEntity>(string id) where TEntity : GraphModel;
        Task<TrackingModel> Put<TEntity>(ActivityContext context, TEntity entity) where TEntity : GraphModel;
        Task<TrackingModel> Put(ActivityContext context, TrackingModel entity);
    }
}
