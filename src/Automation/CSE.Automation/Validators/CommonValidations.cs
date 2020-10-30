using CSE.Automation.Graph;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSE.Automation.Validators
{
    class EmailListValidator : PropertyValidator
    {
        const string _emailRegexPattern = @"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";
        private readonly Regex _emailRegex;

        public EmailListValidator()
            : base("{PropertyName} must contain only email addresses.")
        {
            _emailRegex = new Regex(_emailRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var isValid = true;
            if (context.PropertyValue is string field)
            {
                var tokens = field.Split(',', ';');
                foreach (var token in tokens)
                {
                    if (_emailRegex.Match(token).Success == false)
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }

    /*
    class AADServicePrincipalNameValidator : PropertyValidator
    {
        IGraphHelper<User> _graphHelper;

        public AADServicePrincipalNameValidator(IGraphHelper<User> graphHelper)
            : base("{PropertyName} is not a valid Service Principal Name in this Directory.")
        {
            _graphHelper = graphHelper;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is string field)
            {
                var tokens = field.Split(',', ';');
                var errorsFound = false;
                foreach (var token in tokens)
                {
                    var items = _graphHelper.GetGraphObject(token).Result;
                    if (items is null)
                    {
                        
                        errorsFound = true;
                    }
                }

                return errorsFound;
            }

            return true;



        }
    }
    */

    static class CommonValidations
    {
        public static IRuleBuilderOptions<T, TProperty> HasOnlyEmailAddresses<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new EmailListValidator());
        }

        /*
        public static IRuleBuilderOptions<T, TProperty> HasValidAADServicePrincipalNames<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, IGraphHelper<User> graphHelper)
        {
            return ruleBuilder.SetValidator(new AADServicePrincipalNameValidator(graphHelper));
        }
        */
    }
}
