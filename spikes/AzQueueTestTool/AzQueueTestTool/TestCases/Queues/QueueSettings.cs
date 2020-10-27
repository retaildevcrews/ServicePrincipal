using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AzQueueTestTool.TestCases.Queues
{
    public class QueueSettings : IDisposable
    {
        public string StorageConnectionString { get; }

        public QueueSettings()
        {
            StorageConnectionString = ConfigurationManager.AppSettings.Get("storageConnectionString");
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
