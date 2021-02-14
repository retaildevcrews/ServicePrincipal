using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.Mocks
{
    internal class NoopActivityService : IActivityService
    {
        public Task<ActivityHistory> Put(ActivityHistory document)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityHistory> Get(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId)
        {
            throw new NotImplementedException();
        }

        public ActivityContext CreateContext(string name, string correlationId = null, bool withTracking = false)
        {
            throw new NotImplementedException();
        }
    }
}
