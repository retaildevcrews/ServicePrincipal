using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CSE.Automation.Model
{
    public class AuditEntry
    {
        [JsonProperty(PropertyName = "id")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "actionType")]
        public string ActionType { get; set; }

        [JsonProperty(PropertyName = "actionReason")]
        public string ActionReason { get; set; }

        [JsonProperty(PropertyName = "actionDateTime")]
        public DateTime ActionDateTime { get; set; }
        [JsonProperty(PropertyName = "auditMonthYear")]
        public string AuditMonthYear { get; set; }

        [JsonProperty(PropertyName = "targetObject")]
        public Object TargetObject { get; set; }

        public AuditEntry()
        {
        }

        public AuditEntry(object originalDocument)
        {
            if (originalDocument is null)
                throw new ArgumentNullException(nameof(originalDocument));

            this.TargetObject = originalDocument;
            this.ActionDateTime = DateTime.UtcNow;
            string currentMonth = this.ActionDateTime.Month.ToString(new CultureInfo("en-US"));
            string currentYear = this.ActionDateTime.Year.ToString(new CultureInfo("en-US"));
            this.AuditMonthYear = currentMonth + "_" + currentYear;
        }
        public bool Validate(AbstractValidator<AuditEntry> validator, out IEnumerable<ValidationFailure> errors)
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

    public class CreateAuditValidator : AbstractValidator<AuditEntry>
    {
        public CreateAuditValidator()
        {
            RuleFor(x => x.TargetObject).NotNull();
            RuleFor(x => x.CorrelationId).NotNull().NotEmpty();
            RuleFor(x => x.ActionReason).NotNull().NotEmpty();
            RuleFor(x => x.ActionType).NotNull().NotEmpty();
            RuleFor(x => x.ActionDateTime).NotEqual(DateTime.MinValue);
        }
    }
}

