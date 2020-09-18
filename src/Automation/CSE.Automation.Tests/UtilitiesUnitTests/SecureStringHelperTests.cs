using System;
using System.Security;
using CSE.Automation.Utilities;
using Xunit;

namespace CSE.Automation.Tests.UtilitiesUnitTests
{
    public class SecureStringHelperTests
    {
        [Fact]
        public void ConvertToUnsecureString_SuccessfullyUnsecuresString()
        {
            var secretString = "super secret";
            var secureString = new SecureString();

            foreach (var character in secretString)
            {
                secureString.AppendChar(character);
            }

            var unsecureString = SecureStringHelper.ConvertToUnsecureString(secureString);

            Assert.IsNotType<SecureString>(unsecureString);
        }

        [Fact]
        public void ConvertToUnsecureString_WhenStringNullOrEmpty_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SecureStringHelper.ConvertToUnsecureString(null));

            Assert.Throws<ArgumentNullException>(() => SecureStringHelper.ConvertToUnsecureString(new SecureString()));
        }

        [Fact]
        public void ConvertToSecureString_SuccessfullySecuresString()
        {
            var secretString = "super duper secret";

            var secureString = SecureStringHelper.ConvertToSecureString(secretString);

            Assert.IsType<SecureString>(secureString);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ConvertSecureString_WhenStringNullOrEmpty_ThrowsArgumentNullException(string stringToSecure)
        {
            Assert.Throws<ArgumentNullException>(() => SecureStringHelper.ConvertToSecureString(stringToSecure));
        }
    }
}
