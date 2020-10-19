using CSE.Automation.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    internal interface IObjectTrackingService
    {
        Task<TrackingModel> Get<TEntity>(string id);
        Task<TEntity> GetAndUnwrap<TEntity>(string id) where TEntity : GraphModel;
        Task<TrackingModel> Put<TEntity>(TEntity entity) where TEntity : GraphModel;
    }
}
