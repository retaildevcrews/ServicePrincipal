using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
