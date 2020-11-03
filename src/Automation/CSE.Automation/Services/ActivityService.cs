using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

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
            document = await repository.CreateDocumentAsync(document).ConfigureAwait(false);

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

        /// <summary>
        /// Create an instance of an ActivityHistory model
        /// </summary>
        /// <param name="name">Name of the activity</param>
        /// <param name="withTracking">True if the activity is tracked in ActivityHistory</param>
        /// <returns>A new instance of <see cref="ActivityHistory"/>.</returns>
        public ActivityContext CreateContext(string name, bool withTracking = false)
        {
            var now = DateTimeOffset.Now;
            var document = new ActivityHistory
            {
                Created = now,
                YearMonth = now.ToString("yyyyMM", CultureInfo.CurrentCulture),
                Name = name,
            };

            // we need the id of the run when we initiate
            repository.GenerateId(document);

            return new ActivityContext(withTracking ? this : null)
            {
                Activity = document,
            };
        }
    }
}
