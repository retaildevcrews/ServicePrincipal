// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CSE.Automation.Interfaces;

namespace CSE.Automation.Model
{
    public abstract class SettingsBase : ISettingsValidator
    {
        public virtual void Validate() { }
        private readonly ISecretClient secretClient;

        protected SettingsBase(ISecretClient secretClient)
        {
            this.secretClient = secretClient;
        }

        protected string GetSecret([CallerMemberName] string name = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var propInfo = this.GetType().GetProperty(name);
            if (propInfo == null)
            {
                throw new ArgumentOutOfRangeException($"Failed to get property information for property '{name}'");
            }

            var attr = propInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(SecretAttribute));

            return attr != null
                ? GetSecretInternal(attr.ConstructorArguments.First().Value.ToString())
                : GetSecretInternal(propInfo.Name);
        }

        private string GetSecretInternal(string name)
        {
            return this.secretClient.GetSecretValue(name);
        }
    }
}
