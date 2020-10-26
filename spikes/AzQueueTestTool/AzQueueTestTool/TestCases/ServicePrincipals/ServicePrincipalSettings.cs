using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AzQueueTestTool.TestCases.ServicePrincipals
{
    public class ServicePrincipalSettings : IDisposable
    {
        public string ServicePrincipalPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("prefix");
            }
        }
        public string ServicePrincipalBaseName
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("baseName");
            }
        }

        public string ClientID
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("clientId");
            }
        }

        public string ClientSecret
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("clientSecret");
            }
        }

        public string TenantId
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("tenantId");
            }
        }

        public int NumberOfSPObjectsToCreatePerTestCase => int.Parse(ConfigurationManager.AppSettings.Get("numberOfServicePrincipalObjectsToCreatePerTestCase"));

        private List<string> _targetTestCaseList;
        public List<string> TargetTestCaseList => ConfigurationManager.AppSettings.Get("TargetTestCase").Split(',').Select(s => s.Trim()).ToList();
        
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
