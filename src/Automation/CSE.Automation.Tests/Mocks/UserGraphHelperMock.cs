using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.Mocks
{
    internal class UserGraphHelperMock : IGraphHelper<User>
    {
        public List<User> Data { get; set; } = new List<User>();

        public Task<(GraphOperationMetrics metrics, IEnumerable<User> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetEntityWithOwners(string id)
        {
            var user = Data.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            return await Task.FromResult(user);
        }

        public Task PatchGraphObject(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
