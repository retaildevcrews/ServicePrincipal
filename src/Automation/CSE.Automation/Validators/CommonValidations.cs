﻿using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace CSE.Automation.Validators
{
    internal class EmailListValidator : PropertyValidator
    {
        private const string _emailRegexPattern = @"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";
        private readonly Regex _emailRegex;

        public EmailListValidator()
            : base("{PropertyName} must contain only email addresses.")
        {
            _emailRegex = new Regex(_emailRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is string field)
            {
                var tokens = field.Split(',', ';');
                var errorsFound = false;
                foreach (var token in tokens)
                {
                    if (_emailRegex.Match(token).Success == false) { errorsFound = true; }
                }

                return errorsFound;
            }

            return true;



        }
    }

    internal static class CommonValidations
    {
        public static IRuleBuilderOptions<T, TProperty> HasOnlyEmailAddresses<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new EmailListValidator());
        }
    }
}
