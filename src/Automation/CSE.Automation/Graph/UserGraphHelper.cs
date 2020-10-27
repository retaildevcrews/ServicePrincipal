using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Graph
{
    class UserGraphHelper : GraphHelperBase<User>
    {
        public UserGraphHelper(GraphHelperSettings settings, IAuditService auditService, ILogger<UserGraphHelper> logger) : base(settings, auditService, logger) { }

        public async override Task<(string, IEnumerable<User>)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null)
        {
            throw new NotImplementedException();
        }

        public async override Task<User> GetGraphObject(string id)
        {
            try
            {
                var value = await graphClient.Users[id]
                    .Request()
                    .GetAsync()
                    .ConfigureAwait(false);
                return value;
            }
            catch (Exception)
            {
                return null;
            }


        }

        public async override Task PatchGraphObject(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
