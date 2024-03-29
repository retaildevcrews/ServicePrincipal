﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private readonly ILogger logger;

        public ObjectTrackingService(IObjectTrackingRepository objectRepository, ILogger<ObjectTrackingService> logger)
        {
            this.objectRepository = objectRepository;
            this.logger = logger;
        }

        public async Task<TrackingModel> Get<TEntity>(string id)
            where TEntity : GraphModel
        {
            var entity = await objectRepository
                                    .GetByIdAsync(id, EntityToObjectType(typeof(TEntity)).ToString())
                                    .ConfigureAwait(false);

            return entity;
        }

        public async Task<TEntity> GetAndUnwrap<TEntity>(string id)
            where TEntity : GraphModel
        {
            var entity = await objectRepository
                                    .GetByIdAsync(id, EntityToObjectType(typeof(TEntity)).ToString())
                                    .ConfigureAwait(false);

            return TrackingModel.Unwrap<TEntity>(entity);
        }

        public async Task<TrackingModel> Put(ActivityContext context, TrackingModel entity)
        {
            objectRepository.GenerateId(entity);
            entity.CorrelationId = context.CorrelationId;
            entity.LastUpdated = DateTimeOffset.Now;
            return await objectRepository.UpsertDocumentAsync(entity).ConfigureAwait(false);
        }

        public async Task<TrackingModel> Put<TEntity>(ActivityContext context, TEntity entity)
            where TEntity : GraphModel
        {
            // get the wrapper if it exists
            TrackingModel wrapper = await Get<TEntity>(entity.Id).ConfigureAwait(false);

            var now = DateTimeOffset.Now;
            var newWrapper = new TrackingModel<TEntity>
            {
                Id = wrapper?.Id,
                CorrelationId = context.CorrelationId,
                Created = wrapper?.Created ?? now,
                LastUpdated = now,
                TypedEntity = entity,
            };
            objectRepository.GenerateId(newWrapper);
            return await objectRepository.UpsertDocumentAsync(newWrapper).ConfigureAwait(false);
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
