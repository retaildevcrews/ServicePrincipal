using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IRepository
    {
        Task<bool> Test();
        Task Reconnect(bool force = false);
        string Id { get; }
    }
}
