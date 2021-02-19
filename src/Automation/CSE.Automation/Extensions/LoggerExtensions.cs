// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Extensions
{
    public static class LoggerExtensions
    {
        public static IDisposable BeginScopeWith(this ILogger logger, object values)
        {
            Debug.Assert(logger != null, $"{nameof(BeginScopeWith)}: Logger was null");

            var dictionary = DictionaryHelper.GetValuesAsDictionary(values);
            return logger?.BeginScope(dictionary);
        }
    }
}
