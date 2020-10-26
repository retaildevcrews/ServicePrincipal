namespace CSE.Automation.Interfaces
{
    internal interface ICosmosDBSettings : ISettingsValidator
    {
        string Uri { get; }
        string Key { get; }
        string DatabaseName { get; }
    }
}
