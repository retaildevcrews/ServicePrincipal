// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;

namespace CSE.Automation.Model.Validators
{
    /*
    class AADServicePrincipalNameValidator : PropertyValidator
    {
        IGraphHelper<User> graphHelper;

        public AADServicePrincipalNameValidator(IGraphHelper<User> graphHelper)
            : base("{PropertyName} is not a valid Service Principal Name in this Directory.")
        {
            graphHelper = graphHelper;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is string field)
            {
                var tokens = field.Split(',', ';');
                var errorsFound = false;
                foreach (var token in tokens)
                {
                    var items = graphHelper.GetGraphObject(token).Result;
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

    internal static class CommonValidations
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
