using CSE.Automation.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CSE.Automation.Validators
{
    public class ServicePrincipalModelValidator : AbstractValidator<ServicePrincipalModel>, IModelValidator<ServicePrincipalModel>
    {
        public ServicePrincipalModelValidator()
        {
            Include(new GraphModelValidator());
            RuleFor(m => m.AppId)
                .NotEmpty()
                .MaximumLength(Constants.MaxStringLength);
            //RuleFor(m => m.AppDisplayName)
            //    .NotEmpty()
            //    .MaximumLength(Constants.MaxStringLength);
            //RuleFor(m => m.DisplayName)
            //    .NotEmpty()
            //    .MaximumLength(Constants.MaxStringLength);
            RuleFor(m => m.Notes)
                .NotEmpty()
                .HasOnlyEmailAddresses()
                .MaximumLength(Constants.MaxStringLength);
        }

    }
}
