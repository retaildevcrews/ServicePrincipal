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
        private readonly IObjectTrackingRepository _objectRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger _logger;

        public ObjectTrackingService(IObjectTrackingRepository objectRepository, IAuditService auditService, ILogger<ObjectTrackingService> logger)
        {
            _objectRepository = objectRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<TrackingModel> Get<TEntity>(string id) where TEntity : GraphModel
        {
            var entity = await _objectRepository
                                    .GetByIdAsync(id, EntityToObjectType(typeof(TEntity)).ToString().ToCamelCase())
                                    .ConfigureAwait(false);

            return entity;
        }

        public async Task<TEntity> GetAndUnwrap<TEntity>(string id) where TEntity : GraphModel
        {
            var entity = await _objectRepository
                                    .GetByIdAsync(id, EntityToObjectType(typeof(TEntity)).ToString().ToCamelCase())
                                    .ConfigureAwait(false);

            return entity.Entity as TEntity;
        }

        public async Task<TrackingModel> Put(TrackingModel entity)
        {
            _objectRepository.GenerateId(entity);
            entity.LastUpdated = DateTimeOffset.Now;
            return await _objectRepository.UpsertDocumentAsync(entity).ConfigureAwait(false);
        }

        public async Task<TrackingModel> Put<TEntity>(TEntity entity) where TEntity : GraphModel
        {
            var model = new TrackingModel<TEntity>
            {
                Created = DateTimeOffset.Now,
                LastUpdated = DateTimeOffset.Now,
                TypedEntity = entity,
            };
            _objectRepository.GenerateId(model);
            return await _objectRepository.UpsertDocumentAsync(model).ConfigureAwait(false);
        }

        /// <summary>
        /// Type map between enumeration value and Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static ObjectType EntityToObjectType(Type type)
        {
            if (type == typeof(ServicePrincipalModel))
            {
                return ObjectType.ServicePrincipal;
            }
            throw new ApplicationException($"Unexpected tracking object type {type.Name}");
        }
    }
}
