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
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class SecretAttribute : Attribute
    {
        private readonly string name;

        public SecretAttribute(string name)
        {
            this.name = name.Trim('%');
        }

        public string SecretName => name;
    }
}
