﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using FluentValidation;

namespace CSE.Automation.Validators
{
    public class AuditEntryValidator : AbstractValidator<AuditEntry>, IModelValidator<AuditEntry>
    {
        public AuditEntryValidator()
        {
            RuleFor(x => x.CorrelationId).NotNull().NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.Reason).NotEmpty();
            RuleFor(x => x.Timestamp).NotEmpty().NotEqual(DateTimeOffset.MinValue);
            RuleFor(x => x.AuditYearMonth).NotEmpty();
            RuleFor(x => x.AttributeName).NotEmpty();
            RuleFor(x => x.ExistingAttributeValue).NotEmpty();
        }
    }
}