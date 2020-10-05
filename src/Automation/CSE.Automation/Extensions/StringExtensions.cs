#nullable enable
using System;
using System.ComponentModel;

namespace CSE.Automation.Extensions
{
    static class StringExtensions
    {
        public static TEnum As<TEnum>(this string? value, TEnum? defaultValue=null) where TEnum : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue == null) throw new InvalidEnumArgumentException($"Cannot coerce {value} to {typeof(TEnum).Name}");
                return defaultValue.Value;
            }

            if (Enum.TryParse<TEnum>(value, true, out var enumValue)) return enumValue;
            throw new InvalidEnumArgumentException($"Cannot coerce {value} to {typeof(TEnum).Name}");
        }
    }
}
