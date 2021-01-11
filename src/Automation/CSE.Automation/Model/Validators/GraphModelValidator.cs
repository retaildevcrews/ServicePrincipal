// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Mail;
using System.Text.Json;
using CSE.Automation.Interfaces;
using FluentValidation;

namespace CSE.Automation.Model.Validators
{
    public class GraphModelValidator : AbstractValidator<GraphModel>, IModelValidator<GraphModel>
    {
        public GraphModelValidator()
        {
            var minDate = new DateTimeOffset(new DateTime(1990, 1, 1), TimeSpan.Zero);
            RuleFor(m => m.Id)
                .NotEmpty()
                .MaximumLength(Constants.MaxStringLength);
            RuleFor(m => m.Created)
                .NotEmpty()
                .GreaterThan(minDate);
            RuleFor(m => m.LastUpdated)
                .GreaterThan(minDate);
            RuleFor(m => m.Deleted)
                .GreaterThan(minDate);
            RuleFor(m => m)
                .Must(BeValidModelDateSequence)
                .WithMessage("'Created', 'Deleted', 'LastUpdated'");
            RuleFor(m => m.ObjectType)
                .IsInEnum();
        }

        protected static bool BeValidModelDateSequence(GraphModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Deleted.HasValue)
            {
                return model.LastUpdated >= model.Deleted && model.Deleted >= model.Created;
            }
            else if (model.LastUpdated.HasValue)
            {
                return model.LastUpdated >= model.Created;
            }

            return true;
        }

        protected static bool BeValidJson(string json)
        {
            try
            {
                JsonSerializer.Deserialize<object>(json); // bring in static settings
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
