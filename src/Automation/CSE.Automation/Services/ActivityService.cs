// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Services
{
    internal class ActivityService : IActivityService
    {
        private readonly IActivityHistoryRepository repository;
        private readonly ILogger logger;

        public ActivityService(IActivityHistoryRepository repository, ILogger<ActivityService> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <summary>
        /// Save the ActivityHistory document.
        /// </summary>
        /// <param name="document">The instance of <see cref="ActivityHistory"/> to save.</param>
        /// <returns>The updated instance of <see cref="ActivityHistory"/>.</returns>
        public async Task<ActivityHistory> Put(ActivityHistory document)
        {
            repository.GenerateId(document);
            document.LastUpdated = DateTimeOffset.Now;
            document = await repository.UpsertDocumentAsync(document).ConfigureAwait(false);

            logger.LogInformation($"Saved history for Run {document.Id}");
            return document;
        }

        /// <summary>
        /// Given a document Id, return an instance of the ActivityHistory document.
        /// </summary>
        /// <param name="id">Unique Id of the document.</param>
        /// <returns>An instance of <see cref="ActivityHistory"/> document or null.</returns>
        public async Task<ActivityHistory> Get(string id)
        {
            return await repository.GetByIdAsync(id, id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId)
        {
            return await repository.GetCorrelated(correlationId).ConfigureAwait(false);
        }

        /// <summary>
        /// Create an instance of an ActivityHistory model
        /// </summary>
        /// <param name="name">Name of the activity</param>
        /// <param name="correlationId">Correlation Id of the activity</param>
        /// <param name="withTracking">True if the activity is tracked in ActivityHistory</param>
        /// <returns>A new instance of <see cref="ActivityHistory"/>.</returns>
        public ActivityContext CreateContext(string name, string correlationId = null, bool withTracking = false)
        {
            var now = DateTimeOffset.Now;

            correlationId ??= Guid.NewGuid().ToString();

            var document = new ActivityHistory
            {
                CorrelationId = correlationId,
                Created = now,
                Name = name,
                Status = ActivityHistoryStatus.Running,
            };

            // we need the id of the run when we initiate
            repository.GenerateId(document);

            if (withTracking)
            {
                document = this.Put(document).Result;
            }

            return new ActivityContext(withTracking ? this : null)
            {
                Activity = document,
            }.WithCorrelationId(correlationId);
        }
    }
}
