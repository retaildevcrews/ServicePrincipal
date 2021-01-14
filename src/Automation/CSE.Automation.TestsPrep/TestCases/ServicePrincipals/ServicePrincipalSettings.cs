using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CSE.Automation.TestsPrep.TestCases.ServicePrincipals
{
    internal class ServicePrincipalSettings : IDisposable
    {
        private ConfigurationHelper _configHelper;

        public IConfigurationRoot Config
        {
            get
            {
                return _configHelper.Config;
            }
        }
        public string ServicePrincipalPrefix
        {
            get
            {
                return _configHelper.Config["servicePrincipalPrefix"];
            }
        }
     
        public string UserPrefix
        {
            get
            {
                return _configHelper.Config["aadUserServicePrincipalPrefix"];
            }
        }
    
     
        public string ClientID
        {
            get
            {
                return _configHelper.GraphSettings.GraphAppClientId;
            }
        }

        public string ClientSecret
        {
            get
            {
                return _configHelper.GraphSettings.GraphAppClientSecret;
            }
        }

        public string TenantId
        {
            get
            {
                return _configHelper.GraphSettings.GraphAppTenantId;
            }
        }

        public int NumberOfSPObjectsToCreatePerTestCase => int.Parse(_configHelper.Config["numberOfServicePrincipalObjectsToCreatePerTestCase"]);

        public int NumberOfUsersToCreatePerTestCase => int.Parse(_configHelper.Config["numberOfUsersToCreatePerTestCase"]);

        public List<string> TargetTestCaseList => _configHelper.Config["TargetTestCase"].Split(',').Select(s => s.Trim()).ToList();
        
        public ServicePrincipalSettings(ConfigurationHelper configHelper)
        {
            _configHelper = configHelper;
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
