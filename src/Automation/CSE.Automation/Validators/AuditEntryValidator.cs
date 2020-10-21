using System;
using CSE.Automation.Model;
using FluentValidation;

namespace CSE.Automation.Validators
{
    public class AuditEntryValidator : AbstractValidator<AuditEntry>, IModelValidator<AuditEntry>
    {
        public AuditEntryValidator()
        {
            RuleFor(x => x.TargetObject).NotNull();
            RuleFor(x => x.CorrelationId).NotNull().NotEmpty();
            RuleFor(x => x.ActionReason).NotNull().NotEmpty();
            RuleFor(x => x.ActionType).NotNull().NotEmpty();
            RuleFor(x => x.ActionDateTime).NotEqual(DateTimeOffset.MinValue);
        }
    }
}
