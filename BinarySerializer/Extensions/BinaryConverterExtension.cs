using System;
using System.Collections.Generic;
using Drenalol.Binary.Converters;

namespace Drenalol.Binary.Extensions
{
    public static class BinaryConverterExtension
    {
        public static bool TryConvert(this IDictionary<Type, BinaryConverter> converters, Type type, object o, out byte[] result)
        {
            if (converters.TryGetValue(type, out var converter))
            {
                result = converter.ConvertTo(o);
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryConvertBack(this IDictionary<Type, BinaryConverter> converters, Type type, ReadOnlySpan<byte> span, out object result)
        {
            if (converters.TryGetValue(type, out var converter))
            {
                result = converter.ConvertBackTo(span);
                return true;
            }

            result = default;
            return false;
        }
    }
}