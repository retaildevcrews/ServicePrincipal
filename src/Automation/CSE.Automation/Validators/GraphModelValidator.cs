using System;
using System.Collections.Generic;
using CSE.Automation.Model;
using FluentValidation;

namespace CSE.Automation.Validators
{
    public class GraphModelValidator<T> : AbstractValidator<T> where T : GraphModel
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
                .Must(BeValidModelDateSequence);
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
    }
}
