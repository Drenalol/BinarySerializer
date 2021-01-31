using System;
using System.Buffers;
using Drenalol.Binary.Attributes;

namespace Drenalol.Binary.Infrastracture.Implementations
{
    public sealed class StandartBinaryDeserializer : IBinaryDeserializer
    {
        public IDeserializeResult<TId, TData> Deserialize<TId, TData>(in ReadOnlySequence<byte> sequence, BinarySerializerContext context) where TId : struct where TData : new()
        {
            StandartDeserializerResult<TId, TData> result = InternalDeserialize(sequence, typeof(TId), typeof(TData), context);
            return result;
        }

        private static InternalDeserializeResult InternalDeserialize(in ReadOnlySequence<byte> sequence, Type typeId, Type typeData, BinarySerializerContext context)
        {
            var id = typeId != null ? Activator.CreateInstance(typeId) : null;
            var data = Activator.CreateInstance(typeData);
            var length = 0;
            var propertyIndex = 0;
            var examined = 0;

            foreach (var property    in  context.ReflectionData.Properties)
            {
                object value;
                var sliceLength = property.Attribute.BinaryDataType switch
                {
                    BinaryDataType.MetaData => property.Attribute.Length,
                    BinaryDataType.Id => property.Attribute.Length,
                    BinaryDataType.Length => property.Attribute.Length,
                    BinaryDataType.Body => length,
                    BinaryDataType.Compose => length,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var slice = sequence.Slice(propertyIndex, sliceLength);

                if (property.Attribute.BinaryDataType == BinaryDataType.Compose && !property.IsPrimitive)
                {
                    var composeContext = BinaryCache.GetOrAddContext(property.Type, context.BitConverterHelper);
                    var composeResult = InternalDeserialize(slice, null, property.Type, composeContext);
                    value = composeResult.Data;
                    
                    if (composeContext.ReflectionData.IdProperty != null)
                        id = composeResult.Id;
                }
                else
                    value = context.BitConverterHelper.ConvertFromBytes(slice, property.Type, property.Attribute.Reverse);

                switch (property.Attribute.BinaryDataType)
                {
                    case BinaryDataType.Length:
                        length = value is int lengthValue ? lengthValue : Convert.ToInt32(value);
                        break;
                    case BinaryDataType.Id:
                        id = value;
                        break;
                }

                property.Set(data, value);
                propertyIndex += sliceLength;
                examined++;

                if (examined == context.ReflectionData.Properties.Count)
                    break;
            }

            return new InternalDeserializeResult(id, data);
        }

        public class InternalDeserializeResult
        {
            public object Id { get; }
            public object Data { get; }
            
            public InternalDeserializeResult(object id, object data)
            {
                Id = id;
                Data = data;
            }
        }
    }
}