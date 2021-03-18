using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Tests.Mocks
{
    internal class DefaultActivityService : IActivityService
    {
        private readonly ILogger logger;
        public List<ActivityHistory> Data { get; private set; } = new List<ActivityHistory>();

        public DefaultActivityService(ILogger<DefaultActivityService> logger)
        {
            this.logger = logger;
        }

        public async Task<ActivityHistory> Put(ActivityHistory document)
        {
            this.Data.Add(document);
            return await Task.FromResult(document);
        }

        public Task<ActivityHistory> Get(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId)
        {
            throw new NotImplementedException();
        }

        public ActivityContext CreateContext(string name, string source, string correlationId = null, bool withTracking = false)
        {
            var now = DateTimeOffset.Now;

            correlationId ??= Guid.NewGuid().ToString();

            var document = new ActivityHistory
            {
                CorrelationId = correlationId,
                Created = now,
                Name = name,
                Status = ActivityHistoryStatus.Running,
                CommandSource = source,
            };

            // we need the id of the run when we initiate
            //repository.GenerateId(document);

            //if (withTracking)
            //{
            //    document = this.Put(document).Result;
            //}

            var context = new ActivityContext(withTracking ? this : null)
            {
                Activity = document,
            }.WithCorrelationId(correlationId);

            context.LoggingScope = logger.BeginScopeWith(new { correlationId = correlationId, activityId = context.Activity.Id });
            return context;
        }
    }
}
