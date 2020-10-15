using NSubstitute;
using Xunit;
using CSE.Automation.Model;
using FluentValidation;
using System;
using System.Collections.Generic;
using FluentValidation.Results;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ModelValidationTests
    {

        var servicePrincipalValidator = new CreateServicePrincipalValidator();

        [Fact]
        public void ServicePrincipalModelValidate_ReturnsValidationFailuresIfInvalid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name"
            };

            var validationSuccess = servicePrincipal.Validate(servicePrincipalValidator, out IEnumerable<ValidationFailure> errors);

            Assert.False(validationSuccess);
            Assert.Contains(errors, x => x.PropertyName == "Notes");
        }

        [Fact]
        public void ServicePrincipalModelValidate_ThrowsArgumentNullExceptionIfValidatorNotPassedIn()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name"
            };
            var exception = Record.Exception(() => servicePrincipal.Validate(null, out IEnumerable<ValidationFailure> errors));
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ServicePrinciapalModelValidate_ReturnsTrueIfValid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name",
                Id = "fake id",
                Notes = "super fake note"
            };

            var validationSuccess = servicePrincipal.Validate(servicePrincipalValidator, out IEnumerable<ValidationFailure> errors);

            Assert.True(validationSuccess);
            Assert.Null(errors);
        }
    }
}
