using CSE.Automation.Graph;
using CSE.Automation.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CSE.Automation.Validators
{
    public class ServicePrincipalModelValidator : AbstractValidator<ServicePrincipalModel>, IModelValidator<ServicePrincipalModel>
    {
        public ServicePrincipalModelValidator(IGraphHelper<User> graphHelper)
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
                .Custom((field, context) =>
                {
                    field?.Split(',', ';').ToList().ForEach(token =>
                    {
                        if (graphHelper.GetGraphObject(token).Result is null)
                        {
                            context.AddFailure($"{token} is not a valid UserPrincipalName in this directory");
                        }
                    });
                });
        }

    }
}
