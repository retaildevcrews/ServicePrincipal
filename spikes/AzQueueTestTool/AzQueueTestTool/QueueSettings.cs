using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AzQueueTestTool
{
    internal class QueueSettings : IDisposable
    {
        public string StorageConnectionString { get; }
        public int MessageCount { get; }
        public string QueueNames { get; }
        public string MessageBase { get;}
        public List<string> QueueNamesList { get; }
        public QueueSettings()
        {
            StorageConnectionString = ConfigurationManager.AppSettings.Get("storageConnectionString");
            MessageBase = ConfigurationManager.AppSettings.Get("messageBase");
            QueueNames = ConfigurationManager.AppSettings.Get("queueNames");
            QueueNamesList = QueueNames.Split(',').ToList();
            MessageCount = int.Parse(ConfigurationManager.AppSettings.Get("messageCount"));
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
