﻿using NSubstitute;
using Xunit;
using CSE.Automation.Model;
using FluentValidation;
using System;
using System.Collections.Generic;
using FluentValidation.Results;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CSE.Automation.Validators;
using CSE.Automation.Graph;
using Microsoft.Graph;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using Xunit.Abstractions;
using Moq;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ModelValidationTests
    {
        private readonly ITestOutputHelper output;

        public ModelValidationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // Function to create mock instance of UserGraphHelper, which is needed for ServicePrincipalModelValidator instance
        internal static Mock<IGraphHelper<User>> CreateMockUserGraphHelper()
        {
            Mock<IGraphHelper<User>> mockUserGraphHelper = new Mock<IGraphHelper<User>>();
            Task<User> outTask = Task.FromResult(new User());
            mockUserGraphHelper.Setup(x => x.GetGraphObjectWithOwners(It.IsAny<string>())).Returns(outTask);

            return mockUserGraphHelper;
        }

        AbstractValidator<ServicePrincipalModel> servicePrincipalValidator = new ServicePrincipalModelValidator(CreateMockUserGraphHelper().Object);
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
            output.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, x => x.PropertyName == "Notes");
        }

        [Fact]
        public void ServicePrincipalModelValidate_ReturnsTrueIfValid()
        {
            var servicePrincipal = new ServicePrincipalModel
            {
                AppId = "fake app id",
                AppDisplayName = "fake app display name",
                DisplayName = "fake display name",
                Notes = "email@domain.com",
                Id = "fake id",
                Created = new DateTime(2000, 1, 1),
                Deleted = new DateTime(2001, 1, 1),
                LastUpdated = new DateTime(2002, 1, 1),
                ObjectType = ObjectType.ServicePrincipal,
            };

            var results = servicePrincipalValidator.Validate(servicePrincipal);
            output.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
            
            Assert.True(results.IsValid);
            Assert.True(results.Errors.Count == 0);
        }

        [Fact]
        public void AuditEntryModelValidate_ReturnsValidationFailuresIfInvalid()
        {
            var auditItem = new AuditEntry();

            var results = auditEntryValidator.Validate(auditItem);
            output.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

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
                ExistingAttributeValue = "asdf",
                Timestamp = DateTimeOffset.Now
            };

            var results = auditEntryValidator.Validate(auditItem);
            output.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            Assert.True(results.IsValid);
            Assert.True(results.Errors.Count == 0);
        }
    }
}
