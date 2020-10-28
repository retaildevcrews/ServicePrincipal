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
                return ConfigurationManager.AppSettings.Get("servicePrincipalPrefix");
            }
        }
        public string ServicePrincipalBaseName
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("servicePrincipalBaseName");
            }
        }

        public string UserPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("userPrefix");
            }
        }
        public string UserBaseName
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("userBaseName");
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

        public int NumberOfUsersToCreatePerTestCase => int.Parse(ConfigurationManager.AppSettings.Get("numberOfUsersToCreatePerTestCase"));

        public List<string> TargetTestCaseList => ConfigurationManager.AppSettings.Get("TargetTestCase").Split(',').Select(s => s.Trim()).ToList();
        
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
