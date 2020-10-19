using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Mail;
using CSE.Automation.Model;
using FluentValidation;

namespace CSE.Automation.Validators
{
    public class GraphModelValidator : AbstractValidator<GraphModel>, IModelValidator<GraphModel>
    {
        public GraphModelValidator()
        {
            RuleFor(m => m.Id)
                .NotEmpty()
                .MaximumLength(Constants.MaxStringLength);
            RuleFor(m => m.Created)
                .NotEmpty()
                .GreaterThan(new DateTime(1990, 1, 1));
            RuleFor(m => m.LastUpdated)
                .NotEmpty()
                .GreaterThan(new DateTime(1990, 1, 1));
            RuleFor(m => m)
                .Must(BeValidModelDateSequence)
                .WithMessage("'Created', 'Deleted', 'LastUpdated' sequence invalid.");
            RuleFor(m => m.ObjectType)
                .IsInEnum();
            RuleFor(m => m.Status)
                .IsInEnum();
        }

        protected static bool BeValidModelDateSequence(GraphModel model)
        {
            if (model.Deleted == null)
            {
                return model.LastUpdated >= model.Created;
            }
            else
            {
                return model.LastUpdated >= model.Deleted && model.Deleted >= model.Created;
            }
        }

        protected static bool BeValidJson(string json)
        {
            try
            {
                JsonSerializer.Deserialize<object>(json); //bring in static settings
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        protected static bool BeValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email).Address;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
