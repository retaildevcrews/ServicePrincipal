using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class ObjectTrackingServiceMock : IObjectTrackingService 
    {
        public Dictionary<string, TrackingModel> Data { get; private set; } = new Dictionary<string, TrackingModel>();

        public static ObjectTrackingServiceMock Create()
        {
            return new ObjectTrackingServiceMock();
        }

        public ObjectTrackingServiceMock WithData(TrackingModel[] data)
        {
            if (data != null)
            {
                this.Data = data.ToDictionary(x => x.Id);
            }

            return this;
        }

        public async Task<TrackingModel> Get<TEntity>(string id) where TEntity : GraphModel
        {
            this.Data.TryGetValue(id, out TrackingModel item);
            return await Task.FromResult(item);
        }

        public async Task<TEntity> GetAndUnwrap<TEntity>(string id) where TEntity : GraphModel
        {
            var entity = await Get<TEntity>(id);
            return await Task.FromResult(TrackingModel.Unwrap<TEntity>(entity));
        }

        public async Task<TrackingModel> Put<TEntity>(ActivityContext context, TEntity entity) where TEntity : GraphModel
        {
            TrackingModel wrapper = await Get<TEntity>(entity.Id).ConfigureAwait(false);

            var now = DateTimeOffset.Now;
            var newWrapper = new TrackingModel<TEntity>
            {
                Id = wrapper?.Id ?? Get8CharacterRandomString(),
                CorrelationId = context.CorrelationId,
                Created = wrapper?.Created ?? now,
                LastUpdated = now,
                TypedEntity = entity,
            };

            InsertOrUpdate(newWrapper);
            return await Task.FromResult(newWrapper);
        }

        public async Task<TrackingModel> Put(ActivityContext context, TrackingModel entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = ((entity.Entity) as GraphModel)?.Id ?? Get8CharacterRandomString();
            }
            entity.CorrelationId = context.CorrelationId;
            entity.LastUpdated = DateTimeOffset.Now;

            InsertOrUpdate(entity);
            return await Task.FromResult(entity);
        }

        private void InsertOrUpdate(TrackingModel entity)
        {
            this.Data[entity.Id] = entity;
        }
        public static string Get8CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, 8);  // Return 8 character string
        }
}
}
