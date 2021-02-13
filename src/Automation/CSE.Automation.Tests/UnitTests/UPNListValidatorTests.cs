using System.Text;
using System.Text.RegularExpressions;
using CSE.Automation.Model.Validators;
using Xunit;

namespace CSE.Automation.Tests.UnitTests
{
    public class UPNListValidatorTests
    {
        string[] valid_usernames = new string[]
        {
            "j",
            "joe",
            "joe.dingo",
            "joe_",
            "joe-_",
            "joe_dingo",
            "joe-_dingo",
            "joe.dingo.do",
            "joe_dingo_do",
            "joe-dingo-do",
            "joe99.dingo.99",
        };
        string[] valid_domainnames = new string[]
        {
            "j.com",
            "joe.com",
            "joe.cc",
            "j.cc",
            "joe-dingo.com",
            "lab.joe-dingo.com",
            "joe99.cc",
            "joe99.c9",
            "joe-99.c9",
            "my-app.joe-99.c9",
        };


        private string[] invalid_usernames = new string[]
        {
            ".",
            "j.",
            "joe.",
            "-joe",
            "joe_-",
            "joe_dingo.",
            "joe-_dingo.",
            "joe.dingo.do.",
            "joe dingo",
        };

        private string[] invalid_domainnames = new string[]
        {
            "-j.com",
            "joe..com",
            "joe.-c",
            "j.c-",
            "joe_dingo.com",
            "-joe99.c",
            "-0my.joe99.c9",
        };
        public UPNListValidatorTests()
        {
            Initialize();
        }

        void Initialize()
        {

        }


        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_valid_username_format()
        {
            foreach (var item in valid_usernames)
            {
                Assert.True(UPNListValidator.ValidateUserName(item), $"{item} is an invalid username");
            }

        }

        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_invalid_username_format()
        {
            foreach (var item in invalid_usernames)
            {
                Assert.False(UPNListValidator.ValidateUserName(item), $"{item} is a valid username");
            }

        }

        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_valid_domainname_format()
        {
            var regex = new Regex(UPNListValidator.DomainNameRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            foreach (var item in valid_domainnames)
            {
                Assert.True(UPNListValidator.ValidateDomainName(item), $"{item} is an invalid domain name");
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_invalid_domainname_format()
        {
            foreach (var item in invalid_domainnames)
            {
                Assert.False(UPNListValidator.ValidateDomainName(item), $"{item} is a valid domain name");
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_valid_single_upn()
        {
            foreach (var domain in valid_domainnames)
            {
                foreach (var user in valid_usernames)
                {
                    var value = $"{user}@{domain}";
                    Assert.True(UPNListValidator.ValidateUPN(value).Count == 0);
                }
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_valid_multiple_upn()
        {
            var upnList = new StringBuilder();

            foreach (var domain in valid_domainnames)
            {
                foreach (var user in valid_usernames)
                {
                    var value = $"{user}@{domain}";
                    upnList.AppendFormat($"{value};");
                    upnList.AppendFormat($"{value},");
                    upnList.AppendFormat($"{value}, ");
                }
            }

            var result = UPNListValidator.ValidateUPNList(upnList.ToString());
            Assert.True(result.Count == 0, string.Join(';', result));

        }

        [Fact]
        [Trait("Category", "Unit")]
        public void should_be_invalid_single_upn()
        {
            foreach (var domain in invalid_domainnames)
            {
                foreach (var user in valid_usernames)
                {
                    var value = $"{user}@{domain}";
                    Assert.True(UPNListValidator.ValidateUPN(value).Count > 0);
                }
            }

            foreach (var domain in valid_domainnames)
            {
                foreach (var user in invalid_usernames)
                {
                    var value = $"{user}@{domain}";
                    Assert.True(UPNListValidator.ValidateUPN(value).Count > 0);
                }
            }


            foreach (var domain in invalid_domainnames)
            {
                foreach (var user in invalid_usernames)
                {
                    var value = $"{user}@{domain}";
                    Assert.True(UPNListValidator.ValidateUPN(value).Count > 0);
                }
            }
        }
    }
}
