namespace CSE.Automation.Interfaces
{
    interface ICosmosDBSettings : ISettingsValidator
    {
        string Uri { get; }
        string Key { get; }
        string DatabaseName { get; }
    }
}
