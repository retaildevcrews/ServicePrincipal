using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IConfigService<TConfig> where TConfig : class
    {
        Task<TConfig> Put(TConfig newDocument);
        TConfig Get(string id, byte[] defaultConfig);
    }
}
