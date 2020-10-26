namespace CSE.Automation.Interfaces
{
    public interface IServiceResolver
    {
        T GetService<T>(string keyName);
    }

}
