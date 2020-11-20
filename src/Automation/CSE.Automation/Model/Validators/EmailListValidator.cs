// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    internal class EmailListValidator : PropertyValidator
    {
        private const string EmailRegexPattern = @"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";
        private readonly Regex emailRegex;

        public EmailListValidator()
            : base("{PropertyName} must contain only email addresses.")
        {
            emailRegex = new Regex(EmailRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var isValid = true;
            if (context.PropertyValue is string field)
            {
                var tokens = field.Split(',', ';');
                foreach (var token in tokens)
                {
                    if (emailRegex.Match(token).Success == false)
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }
}
