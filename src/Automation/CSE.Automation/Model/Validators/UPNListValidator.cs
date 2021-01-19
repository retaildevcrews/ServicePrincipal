// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation.Validators;

namespace CSE.Automation.Model.Validators
{
    internal class UPNListValidator : PropertyValidator
    {
        // internal const string UserNameRegexPattern = @"^(\w[\w\-\.]*[^\.\-])$";
        internal const string UserNameRegexPattern = @"([ \~|\!|\#|\$|\%|\^|\&|\*|\(|\)|\+|\=|\[|\]|\{|\}|\\|\/|\||\;|\:|""|\<|\>|\?|\,])";
        internal const string DomainNameRegexPattern = @"^(([0-9a-zA-Z][-0-9a-zA-Z]*[0-9a-zA-Z]*\.)+[0-9a-zA-Z][-0-9a-zA-Z]{0,22}[0-9a-zA-Z])$";

        private static readonly Regex UserNameRegex = new Regex(UserNameRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex DomainNameRegex = new Regex(DomainNameRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        public UPNListValidator()
            : base("{PropertyName} must contain only UPNs.\n{errorMessage}")
        {
        }

        internal static bool ValidateUserName(string value)
        {
            if (value.StartsWith('.')
                || value.StartsWith('-')
                || value.EndsWith('.')
                || value.EndsWith('-'))
            {
                return false;
            }
            else if (value.Contains("..", StringComparison.Ordinal))
            {
                return false;
            }
            else
            {
                // the regex looks for invalid characters, so invert the Success
                return !UserNameRegex.Match(value).Success;
            }
        }

        internal static bool ValidateDomainName(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && DomainNameRegex.Match(value).Success;
        }

        internal static IList<string> ValidateUPN(string item)
        {
            var errors = new List<string>();

            // Split item into username and domain name, validate each separately
            var tokens = item.Trim().Split('@');
            switch (tokens.Length)
            {
                case 2:
                    if (ValidateUserName(tokens[0]) == false)
                    {
                        errors.Add($"Username ({tokens[0]}) is invalid in UPN");
                    }

                    if (ValidateDomainName(tokens[1]) == false)
                    {
                        errors.Add($"Domain name ({tokens[1]}) is invalid in UPN");
                    }

                    break;

                default:
                    errors.Add($"'{item}' is an invalid UPN");
                    break;
            }

            return errors;
        }

        internal static IList<string> ValidateUPNList(string value)
        {
            var validationErrors = new List<string>();

            // separated the list into individual terms
            var items = value.Trim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                validationErrors.AddRange(ValidateUPN(item));
            }

            return validationErrors;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is string field)
            {
                var errors = ValidateUPNList(field);
                if (errors.Count > 0)
                {
                    context.MessageFormatter.AppendArgument("errorMessage", string.Join('\n', errors));
                    return false;
                }
            }

            return true;
        }
    }
}
