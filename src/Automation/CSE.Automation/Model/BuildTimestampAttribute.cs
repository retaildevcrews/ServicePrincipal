// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Model
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    internal sealed class BuildTimestampAttribute : System.Attribute
    {
        public string Value { get; }

        public BuildTimestampAttribute(string buildTimestamp)
        {
            this.Value = buildTimestamp;
        }
    }
}
