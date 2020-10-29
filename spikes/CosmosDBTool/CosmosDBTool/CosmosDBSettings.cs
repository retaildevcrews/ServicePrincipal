using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CosmosDBTool
{
    class CosmosDBSettings : IDisposable
    {
        public string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("cosmosDBConnectionString");
            }
        }

        public string DatabaseName
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("databaseName");
            }
        }

        public int Throughput
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("throughput"));
            }
        }

        

        public Dictionary<string, string> ContainerNamePrimaryKey = ConfigurationManager.AppSettings.Get("containerPrimaryKeyPair").TrimEnd(';').Split(';').ToDictionary(item => item.Split('=')[0], item => item.Split('=')[1]);
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
