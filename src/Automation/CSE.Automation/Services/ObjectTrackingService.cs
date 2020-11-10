using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSE.Automation.Services
{
    internal class ObjectTrackingService : IObjectTrackingService
    {
        private readonly IObjectTrackingRepository objectRepository;
        private readonly IAuditService auditService;
        private readonly ILogger logger;

        public ObjectTrackingService(IObjectTrackingRepository objectRepository, IAuditService auditService, ILogger<ObjectTrackingService> logger)
        {
            this.objectRepository = objectRepository;
            this.auditService = auditService;
            this.logger = logger;
        }

        public async Task<TrackingModel> Get<TEntity>(string id)
            where TEntity : GraphModel
        {
            var entity = await objectRepository
                                    .GetByIdAsync(id, EntityToObjectType(typeof(TEntity)).ToString().ToCamelCase())
                                    .ConfigureAwait(false);

            return entity;
        }

        public async Task<TEntity> GetAndUnwrap<TEntity>(string id)
            where TEntity : GraphModel
        {
            var entity = await objectRepository
                                    .GetByIdAsync(id, EntityToObjectType(typeof(TEntity)).ToString().ToCamelCase())
                                    .ConfigureAwait(false);

            return entity?.Entity as TEntity;
        }

        public async Task<TrackingModel> Put(ActivityContext context, TrackingModel entity)
        {
            objectRepository.GenerateId(entity);
            entity.CorrelationId = context.Activity.Id.ToString();
            entity.LastUpdated = DateTimeOffset.Now;
            return await objectRepository.UpsertDocumentAsync(entity).ConfigureAwait(false);
        }

        public async Task<TrackingModel> Put<TEntity>(ActivityContext context, TEntity entity) 
            where TEntity : GraphModel
        {
            var now = DateTimeOffset.Now;
            var model = new TrackingModel<TEntity>
            {
                CorrelationId = context.Activity.Id.ToString(),
                Created = now,
                LastUpdated = now,
                TypedEntity = entity,
            };
            objectRepository.GenerateId(model);
            return await objectRepository.UpsertDocumentAsync(model).ConfigureAwait(false);
        }

        /// <summary>
        /// Type map between enumeration value and Type
        /// </summary>
        /// <param name="type">Type of the model corresponding to an <see cref="ObjectType"/>.</param>
        /// <returns>An enumeration value of type <see cref="ObjectType"/>.</returns>
        public static ObjectType EntityToObjectType(Type type)
        {
            if (type == typeof(ServicePrincipalModel))
            {
                return ObjectType.ServicePrincipal;
            }

            throw new ApplicationException($"Unexpected tracking object type {type.Name}");
        }
    }
}
