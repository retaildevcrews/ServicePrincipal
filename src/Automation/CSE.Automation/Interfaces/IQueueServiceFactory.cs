namespace CSE.Automation.Interfaces
{
    internal interface IQueueServiceFactory
    {
        IAzureQueueService Create(string connectionString, string queueName);
    }
}
