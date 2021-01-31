using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Drenalol.Binary.Converters;
using Drenalol.Binary.Exceptions;
using Drenalol.Binary.Extensions;
using Drenalol.Binary.Models;

namespace Drenalol.Binary.Helpers
{
    public class BitConverterHelper
    {
        private readonly IReadOnlyDictionary<Type, MethodInfo> _builtInConvertersToBytes;
        private readonly IDictionary<Type, BinaryConverter> _customConverters;
        public bool PrimitiveValueReverse = false;

        public BitConverterHelper(IList<BinaryConverter> customConverters)
        {
            _customConverters = new Dictionary<Type, BinaryConverter>();
            _builtInConvertersToBytes = new Dictionary<Type, MethodInfo>
            {
                {typeof(bool), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(bool)})},
                {typeof(char), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(char)})},
                {typeof(double), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(double)})},
                {typeof(short), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(short)})},
                {typeof(int), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(int)})},
                {typeof(long), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(long)})},
                {typeof(float), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(float)})},
                {typeof(ushort), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(ushort)})},
                {typeof(uint), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(uint)})},
                {typeof(ulong), typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), new[] {typeof(ulong)})}
            };

            if (customConverters != null)
                RegisterConverters(customConverters);
        }

        public void RegisterConverters(IEnumerable<BinaryConverter> converters)
        {
            foreach (var converter in converters)
                RegisterConverter(converter);
        }

        public void RegisterConverter(BinaryConverter converter)
        {
            var converterType = converter.GetType();
            var type = converterType.BaseType;

            if (type == null)
                throw BinaryException.ConverterError(converterType.Name);

            var genericType = type.GenericTypeArguments.Single();
            _customConverters.TryAdd(genericType, converter);
        }

        private static byte[] Reverse(byte[] bytes)
        {
            ((Span<byte>) bytes).Reverse();
            return bytes;
        }

        private static Sequence MergeSpans(in ReadOnlySequence<byte> sequences, bool reverse)
        {
            if (!reverse && sequences.IsSingleSegment)
                return Sequence.Create(sequences.FirstSpan, null);

            var sequencesLength = (int) sequences.Length;
            var bytes = ArrayPool<byte>.Shared.Rent(sequencesLength);
            var span = new Span<byte>(bytes, 0, sequencesLength);
            sequences.CopyTo(span);

            if (reverse)
                span.Reverse();

            return Sequence.Create(span, () => ArrayPool<byte>.Shared.Return(bytes));
        }

        public byte[] ConvertToBytes(object propertyValue, Type propertyType, bool? reverse = null)
        {
            switch (propertyValue)
            {
                case null:
                    throw BinaryException.PropertyArgumentIsNull(propertyType.ToString());
                case byte @byte:
                    return new[] {@byte};
                case byte[] byteArray:
                    return reverse.GetValueOrDefault() ? Reverse(byteArray) : byteArray;
                default:
                    try
                    {
                        if (_customConverters.TryConvert(propertyType, propertyValue, out var result))
                            return reverse.GetValueOrDefault() ? Reverse(result) : result;

                        if (!_builtInConvertersToBytes.TryGetValue(propertyType, out var methodInfo))
                            throw BinaryException.ConverterNotFoundType(propertyType.ToString());

                        result = (byte[]) methodInfo.Invoke(null, new[] {propertyValue});
                        return reverse ?? PrimitiveValueReverse ? Reverse(result) : result;
                    }
                    catch (Exception exception) when (!(exception is BinaryException))
                    {
                        throw BinaryException.ConverterUnknownError(propertyType.ToString(), exception.Message);
                    }
            }
        }

        public object ConvertFromBytes(in ReadOnlySequence<byte> slice, Type propertyType, bool? reverse = null)
        {
            if (propertyType == typeof(byte[]))
                return reverse.GetValueOrDefault() ? Reverse(slice.ToArray()) : slice.ToArray();

            if (propertyType == typeof(byte))
                return slice.FirstSpan[0];

            var (span, returnArray) = MergeSpans(slice, propertyType.IsPrimitive ? reverse ?? PrimitiveValueReverse : reverse.GetValueOrDefault());

            try
            {
                if (_customConverters.TryConvertBack(propertyType, span, out var result))
                    return result;

                return propertyType.Name switch
                {
                    nameof(Boolean) => BitConverter.ToBoolean(span),
                    nameof(Char) => BitConverter.ToChar(span),
                    nameof(Double) => BitConverter.ToDouble(span),
                    nameof(Int16) => BitConverter.ToInt16(span),
                    nameof(Int32) => BitConverter.ToInt32(span),
                    nameof(Int64) => BitConverter.ToInt64(span),
                    nameof(Single) => BitConverter.ToSingle(span),
                    nameof(UInt16) => BitConverter.ToUInt16(span),
                    nameof(UInt32) => BitConverter.ToUInt32(span),
                    nameof(UInt64) => BitConverter.ToUInt64(span),
                    _ => throw BinaryException.ConverterNotFoundType(propertyType.ToString())
                };
            }
            catch (Exception exception) when (!(exception is BinaryException))
            {
                throw BinaryException.ConverterUnknownError(propertyType.ToString(), exception.Message);
            }
            finally
            {
                returnArray?.Invoke();
            }
        }
    }
}