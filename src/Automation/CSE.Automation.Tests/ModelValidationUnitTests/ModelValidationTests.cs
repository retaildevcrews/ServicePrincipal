using NSubstitute;
using Xunit;
using CSE.Automation.Model;
using FluentValidation;
using System;
using System.Collections.Generic;
using FluentValidation.Results;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using CSE.Automation.Validators;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ModelValidationTests
    {

        AbstractValidator<ServicePrincipalModel> servicePrincipalValidator = new ServicePrincipalModelValidator();

        [Fact]
        public void ServicePrincipalModelValidate_ReturnsValidationFailuresIfInvalid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name"
            };

            var results = servicePrincipalValidator.Validate(servicePrincipal);
            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, x => x.PropertyName == "Notes");
        }

        [Fact]
        public void ServicePrinciapalModelValidate_ReturnsTrueIfValid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name",
                Notes = "super fake note",
                Id = "fake id",
                Created = new DateTime(2000, 1, 1),
                Deleted = new DateTime(2001, 1, 1),
                LastUpdated = new DateTime(2002, 1, 1),
                ObjectType = ObjectType.ServicePrincipal,
                Status = Status.Remediated
            };

            var results = servicePrincipalValidator.Validate(servicePrincipal);
            Assert.True(results.IsValid);
            Assert.True(results.Errors.Count == 0);
        }
    }
}
