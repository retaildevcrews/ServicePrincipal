
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CSE.Automation.Model
{
    // Used name ServicePrincipalModel to disambiguate from Microsoft.Graph.ServicePrincipal
    public class ServicePrincipalModel : GraphModel
    {
        public string AppId { get; set; }

        public string AppDisplayName { get; set; }

        public string DisplayName { get; set; }

        public string Notes { get; set; }

        public void Validate(AbstractValidator<ServicePrincipalModel> validator)
        {
            if (validator == null)
            {
                throw new ValidationException("Validator must not be null.");
            }
            
            var validationResults = validator.Validate(this);

            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults.Errors);
            }
        }
    }

    // We can add more validation classes as needed for different types of validation for our models
    public class CreateServicePrincipalValidator : AbstractValidator<ServicePrincipalModel>
    {
        public CreateServicePrincipalValidator()
        {
            RuleFor(x => x.Notes).NotNull();
        }
    }
}
