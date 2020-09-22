using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Utilities
{
    public interface IGraphHelper<T>
    {
        Task<(string,IEnumerable<T>)> GetDeltaGraphObjects(string selectFields,string deltaLink, Configuration config);
    }
}