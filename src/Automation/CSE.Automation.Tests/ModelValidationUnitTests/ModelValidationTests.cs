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
using CSE.Automation.Graph;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ModelValidationTests
    {
        // TODO: create real mocked class
        class MockUserGraphHelper : IGraphHelper<User>
        {
            public Task<(GraphOperationMetrics, IEnumerable<User>)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null)
            {
                throw new NotImplementedException();
            }

            public Task<User> GetGraphObject(string id)
            {
                return Task.FromResult(new User());
            }

            public Task PatchGraphObject(User entity)
            {
                throw new NotImplementedException();
            }
        }

        AbstractValidator<ServicePrincipalModel> servicePrincipalValidator = new ServicePrincipalModelValidator(new MockUserGraphHelper());
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
            };

            var results = servicePrincipalValidator.Validate(servicePrincipal);
            Assert.True(results.IsValid);
            Assert.True(results.Errors.Count == 0);
        }

        [Fact]
        public void AuditEntryModelValidate_ReturnsValidationFailuresIfInvalid()
        {
            var auditItem = new AuditEntry();

            var results = auditEntryValidator.Validate(auditItem);
            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, x => x.PropertyName == "CorrelationId");
            Assert.Contains(results.Errors, x => x.PropertyName == "Type");
            Assert.Contains(results.Errors, x => x.PropertyName == "Reason");
        }

        [Fact]
        public void AuditEntryModelValidate_ReturnsTrueIfValid()
        {
            var context = new ActivityContext(null);

            var auditItem = new AuditEntry()
            {
                CorrelationId = "fake correlation id",
                Type = AuditActionType.Change,
                Reason = "fake action reason",
                AuditYearMonth = "qweradsf",
                AttributeName = "asdf",
                ExistingAttributeValue = "asdf"
            };

            var results = auditEntryValidator.Validate(auditItem);

            Assert.True(results.IsValid);
            Assert.True(results.Errors.Count == 0);
        }
    }
}
