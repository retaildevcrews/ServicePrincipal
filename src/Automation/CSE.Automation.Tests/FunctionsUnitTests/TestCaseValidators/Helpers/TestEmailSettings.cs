using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers
{
    public class TestEmailSettings : IEmailSettings, IDisposable
    {
        private IConfigurationRoot _config;
        public string TestEmailBase
        {
            get
            {
                return _config["testEmailBase"];
            }
        }

        public bool IncludeRandomStringToTestEmail
        {
            get
            {
                return false;
            }
        }

        public int TestEmailCount
        {
            get
            {
                return int.Parse(_config["testEmailCount"]);
            }
        }


        public List<string> TestEmailDomainNames => _config["testEmailDomainNames"].Split(',').Select(s => s.Trim()).ToList();


        public TestEmailSettings(IConfigurationRoot config)
        {
            _config = config;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
