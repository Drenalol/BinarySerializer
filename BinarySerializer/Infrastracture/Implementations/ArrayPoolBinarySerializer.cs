using System;
using System.Buffers;
using Drenalol.Binary.Attributes;
using Drenalol.Binary.Exceptions;
using Drenalol.Binary.Models;

namespace Drenalol.Binary.Infrastracture.Implementations
{
    public sealed class ArrayPoolBinarySerializer : IBinarySerializer
    {
        private readonly ArrayPool<byte> _arrayPool;
        
        public ArrayPoolBinarySerializer()
        {
            _arrayPool = ArrayPool<byte>.Create(); 
        }
        
        public ISerializeResult Serialize(object data, BinarySerializerContext context) => InternalSerialize(data, context);

        private ArrayPoolSerializeResult InternalSerialize(object data, BinarySerializerContext context)
        {
            int realLength;
            byte[] serializedBody = null;
            ArrayPoolSerializeResult composeSerializeResult = null;
            var examined = 0;

            if (context.ReflectionData.BodyProperty != null)
            {
                var bodyValue = context.ReflectionData.BodyProperty.Get(data);

                if (bodyValue == null)
                    throw BinaryException.SerializerBodyPropertyIsNull();

                serializedBody = context.BitConverterHelper.ConvertToBytes(bodyValue, context.ReflectionData.BodyProperty.Type, context.ReflectionData.BodyProperty.Attribute.Reverse);
                realLength = CalculateRealLength(context.ReflectionData.LengthProperty, data, context.ReflectionData.MetaLength, serializedBody.Length);
            }
            else if (context.ReflectionData.ComposeProperty != null)
            {
                var composeValue = context.ReflectionData.ComposeProperty.Get(data);

                if (composeValue == null)
                    throw BinaryException.SerializerComposePropertyIsNull();

                if (context.ReflectionData.ComposeProperty.Type.IsPrimitive)
                {
                    serializedBody = context.BitConverterHelper.ConvertToBytes(composeValue, context.ReflectionData.ComposeProperty.Type, context.ReflectionData.ComposeProperty.Attribute.Reverse);
                    realLength = CalculateRealLength(context.ReflectionData.LengthProperty, data, context.ReflectionData.MetaLength, serializedBody.Length);
                }
                else
                {
                    var composeContext = BinaryCache.GetOrAddContext(context.ReflectionData.ComposeProperty.Type, context.BitConverterHelper);
                    composeSerializeResult = InternalSerialize(composeValue, composeContext);
                    realLength = CalculateRealLength(context.ReflectionData.LengthProperty, data, context.ReflectionData.MetaLength, composeSerializeResult.Length);
                }
            }
            else
                realLength = context.ReflectionData.MetaLength;

            var serializeResult = new ArrayPoolSerializeResult(realLength, composeSerializeResult, _arrayPool);
            var bytes = serializeResult.BytesRented;

            foreach (var property in context.ReflectionData.Properties)
            {
                if (!property.Type.IsPrimitive && property.Attribute.BinaryDataType == BinaryDataType.Compose && composeSerializeResult != null)
                {
                    Array.Copy(
                        composeSerializeResult.BytesRented,
                        0,
                        bytes,
                        property.Attribute.Index,
                        composeSerializeResult.Length
                    );
                }
                else
                {
                    var value = property.Attribute.BinaryDataType == BinaryDataType.Body || property.Attribute.BinaryDataType == BinaryDataType.Compose
                        ? serializedBody ?? throw BinaryException.SerializerBodyPropertyIsNull()
                        : context.BitConverterHelper.ConvertToBytes(property.Get(data), property.Type, property.Attribute.Reverse);

                    var valueLength = value.Length;

                    if (property.Attribute.BinaryDataType != BinaryDataType.Body && property.Attribute.BinaryDataType != BinaryDataType.Compose && valueLength > property.Attribute.Length)
                        throw BinaryException.SerializerLengthOutOfRange(property.Type.ToString(), valueLength.ToString(), property.Attribute.Length.ToString());

                    value.CopyTo(bytes, property.Attribute.Index);
                }

                if (++examined == context.ReflectionData.Properties.Count)
                    break;
            }

            return serializeResult;

            static int CalculateRealLength(BinaryMember lengthProperty, object data, int metaLength, object dataLength)
            {
                var lengthValue = lengthProperty.Type == typeof(int)
                    ? dataLength
                    : Convert.ChangeType(dataLength, lengthProperty.Type);

                lengthProperty.Set(data, lengthValue);

                try
                {
                    return (int) lengthValue + metaLength;
                }
                catch (InvalidCastException)
                {
                    return Convert.ToInt32(lengthValue) + metaLength;
                }
            }
        }
    }
}