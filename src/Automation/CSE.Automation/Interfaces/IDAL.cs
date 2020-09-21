using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CSE.Automation.Interfaces
{
    public interface IDAL
    {
        public Task<T> GetById<T>(string Id);
        public Task<IEnumerable<T>> GetPagedAsync<T>(string q, int offset = 0, int limit = 0);
        public Task<IEnumerable<T>> GetAllAsync<T>(TypeFilter filter= TypeFilter.any);
        public Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false);
    }
}
