
using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CSE.Automation.Model
{
    // Used name ServicePrincipalModel to disambiguate from Microsoft.Graph.ServicePrincipal
    public class ServicePrincipalModel : GraphModel, IGraphModel
    {
        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "appDisplayName")]
        public string AppDisplayName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }

        public bool Validate(AbstractValidator<ServicePrincipalModel> validator, out IEnumerable<ValidationFailure> errors)
        {
            errors = null;

            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            var validationResults = validator.Validate(this);

            if (!validationResults.IsValid)
            {
                errors = validationResults.Errors;
                return false;
            }

            return true;
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
