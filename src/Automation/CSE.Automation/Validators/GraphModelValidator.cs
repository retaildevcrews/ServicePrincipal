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
                .MaximumLength(1000);
            RuleFor(m => m.Created)
                .NotEmpty()
                .GreaterThan(new DateTime(1990, 1, 1));
            RuleFor(m => m.LastUpdated)
                .NotEmpty()
                .GreaterThan(new DateTime(1990, 1, 1));
            RuleFor(m => new List<DateTime> { m.Created, m.Deleted, m.LastUpdated })
                .Must(BeValidModelDateSequence)
                .WithMessage("'Created', 'Deleted', 'LastUpdated' sequence invalid.");
            RuleFor(m => m.ObjectType)
                .IsInEnum();
            RuleFor(m => m.Status)
                .IsInEnum();
        }

        protected static bool BeValidModelDateSequence(List<DateTime> dateTimes)
        {
            if (dateTimes[1] == null)
            {
                return dateTimes[2] > dateTimes[0];
            }
            else
            {
                return dateTimes[1] >= dateTimes[0] && dateTimes[2] > dateTimes[1];
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
