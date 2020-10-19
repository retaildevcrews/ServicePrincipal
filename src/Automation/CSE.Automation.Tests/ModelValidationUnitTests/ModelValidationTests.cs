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
        AbstractValidator<AuditEntry> auditEntryValidator = new AuditEntryValidator();

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

        [Fact]
        public void AuditEntryModelValidate_ReturnsValidationFailuresIfInvalid()
        {
            var servicePrincipal = new ServicePrincipalModel()
            {
                Id = "fake id",
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name"
            };

            var auditItem = new AuditEntry(servicePrincipal)
            {
                CorrelationId = null,
                ActionType = string.Empty,
                ActionReason = null
            };

            var results = auditEntryValidator.Validate(auditItem);
            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, x => x.PropertyName == "CorrelationId");
            Assert.Contains(results.Errors, x => x.PropertyName == "ActionType");
            Assert.Contains(results.Errors, x => x.PropertyName == "ActionReason");
        }

        [Fact]
        public void AuditEntryModelValidate_ReturnsTrueIfValid()
        {
            var servicePrincipal = new ServicePrincipalModel()
            {
                Id = "fake id",
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name"
            };

            var auditItem = new AuditEntry(servicePrincipal)
            {
                CorrelationId = "fake correlation id",
                ActionType = "fake action type",
                ActionReason = "fake action reason"
            };

            var results = auditEntryValidator.Validate(auditItem);

            Assert.True(results.IsValid);
            Assert.True(results.Errors.Count == 0);
        }
    }
}
