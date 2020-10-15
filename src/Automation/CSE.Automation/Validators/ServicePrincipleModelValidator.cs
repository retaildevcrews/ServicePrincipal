using System;
using CSE.Automation.Model;
using FluentValidation;

namespace CSE.Automation.Validators
{
    public class ServicePrincipleModelValidator : GraphModelValidator<ServicePrincipalModel>
    {
        public ServicePrincipleModelValidator()
        {
            RuleFor(m => m.AppId)
                .NotEmpty()
                .MaximumLength(1000);
            RuleFor(m => m.AppDisplayName)
                .NotEmpty()
                .MaximumLength(1000);
            RuleFor(m => m.DisplayName)
                .NotEmpty()
                .MaximumLength(1000);
            RuleFor(m => m.Notes)
                .NotEmpty()
                .MaximumLength(1000);
        }
    }
}
