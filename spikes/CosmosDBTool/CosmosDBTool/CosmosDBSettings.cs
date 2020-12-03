using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CosmosDBTool
{
    class CosmosDBSettings : IDisposable
    {
        private string _conectionString;
        private string _databaseName;
        private string _endPoint;
        private string _authKey;
        private Dictionary<string, string> _containerNamePrimaryKey;
        private List<string> _containerNames;
        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_conectionString))
                {
                    _conectionString = ConfigurationManager.AppSettings.Get("cosmosDBConnectionString");
                }
                return _conectionString;
            }
        }

        public string DatabaseName
        {
            get
            {
                if (string.IsNullOrEmpty(_databaseName))
                {

                    _databaseName = ConfigurationManager.AppSettings.Get("databaseName");
                }

                return _databaseName;
                
            }
        }

        public Dictionary<string, string> ContainerNamePrimaryKey
        {
            get
            {
                if (_containerNamePrimaryKey == null)
                {
                    _containerNamePrimaryKey = ConfigurationManager.AppSettings.Get("containerPrimaryKeyPair").TrimEnd(';').Split(';').ToDictionary(item => item.Split('=')[0], item => item.Split('=')[1]);
                }
                return _containerNamePrimaryKey;
            }
        }

        public List<string> ContainerNames
        {
            get
            {
                if (_containerNames == null)
                {
                    _containerNames = ConfigurationManager.AppSettings.Get("containerNames").Split(';').Select(s => s.Trim()).ToList();
                }
                return _containerNames;
            }
        }

        public string Endpoint
        {
            get
            {
                if (string.IsNullOrEmpty(_endPoint))
                {
                    _endPoint = ConnectionString.Split(';').FirstOrDefault(x => x.ToLower().StartsWith("accountendpoint")).Split('=')[1];
                }

                return _endPoint;
            }
        }
        public string AuthKey
        {
            get
            {
                if (string.IsNullOrEmpty(_authKey))
                {
                    _authKey = ConnectionString.Split(';').FirstOrDefault(x => x.ToLower().StartsWith("accountkey"));
                    
                    //Key will endwith  or include "=", so we need to copy a substring 
                    
                    int index = "accountKey=".Length;
                    
                    _authKey = _authKey.Substring(index);


                }

                return _authKey;
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
