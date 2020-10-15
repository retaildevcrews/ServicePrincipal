#nullable enable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSE.Automation.Extensions
{
    static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum As<TEnum>(this string? value, TEnum? defaultValue = null) where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue == null) throw new InvalidEnumArgumentException($"Cannot coerce {value} to {typeof(TEnum).Name}");
                return defaultValue.Value;
            }

            if (Enum.TryParse<TEnum>(value, true, out var enumValue)) return enumValue;
            throw new InvalidEnumArgumentException($"Cannot coerce {value} to {typeof(TEnum).Name}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this string? value, int? defaultValue = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue == null) throw new InvalidEnumArgumentException($"Cannot coerce {value} to int");
                return defaultValue.Value;
            }

            if (int.TryParse(value, out var realValue)) return realValue;
            throw new InvalidEnumArgumentException($"Cannot coerce {value} to int");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ToGuid(this string? value, Guid defaultValue = default)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue == default) throw new InvalidEnumArgumentException($"Cannot coerce {value} to Guid");
                return defaultValue;
            }

            if (Guid.TryParse(value, out var guidValue)) return guidValue;
            throw new InvalidEnumArgumentException($"Cannot coerce {value} to Guid");
        }

        public static bool IsNotNull(this string? value)
        {
            return string.IsNullOrEmpty(value) == false;
        }

        public static bool IsNull(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}
