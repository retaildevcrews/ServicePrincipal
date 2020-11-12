using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSE.Automation.Processors
{
    internal interface IEvaluateProcessor
    {
        Task Evaluate(ActivityContext context, ServicePrincipalModel model);
    }

    internal class EvaluateProcessor : IEvaluateProcessor
    {
        private readonly ILogger logger;
        private readonly IObjectTrackingService objectService;
        private readonly IAuditService auditService;

        public EvaluateProcessor(IObjectTrackingService objectService, IAuditService auditService, ILogger<EvaluateProcessor> logger)
        {
            this.objectService = objectService;
            this.auditService = auditService;
            this.logger = logger;
        }

        /// <summary>
        /// Evaluate the model and determine its disposition.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1. validate model
        ///     if valid, log audit valid, return
        /// 2. Apply Rules
        /// 3. Update object in object tracking
        /// 4. Issue update command to queue
        /// </remarks>
        public async Task Evaluate(ActivityContext context, ServicePrincipalModel model)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
