// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;

namespace CSE.Automation.Model.Validators
{
    internal static class CommonValidations
    {
        public static IRuleBuilderOptions<T, TProperty> HasOnlyEmailAddresses<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new UPNListValidator());
        }
    }
}
