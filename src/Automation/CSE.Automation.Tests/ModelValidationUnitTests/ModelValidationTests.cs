using NSubstitute;
using Xunit;
using CSE.Automation.Model;
using FluentValidation;
using System;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ModelValidationTests
    {

        [Fact]
        public void ServicePrincipalModelValidate_ThrowsValidationExceptionIfInvalid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name"
            };

            Action result = () => servicePrincipal.Validate(new CreateServicePrincipalValidator());

            var exception = Assert.Throws<ValidationException>(result);

            Assert.Contains("'Notes' must not be empty", exception.Message);
        }

        [Fact]
        public void ServicePrinciapalModelValidate_DoesNotThrowExceptionIfValid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name",
                Id = "fake id",
                Notes = "super fake note"
            };

            var exception = Record.Exception(() => servicePrincipal.Validate(new CreateServicePrincipalValidator()));

            Assert.Null(exception);
        }
    }
}
