using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Drenalol.Binary.Converters;
using Drenalol.Binary.Extensions;
using Drenalol.Binary.Helpers;
using Drenalol.Binary.Infrastracture;

namespace Drenalol.Binary
{
    public class BinarySerializer
    {
        private readonly IBinarySerializer _serializer;
        private readonly IBinaryDeserializer _deserializer;
        private readonly BitConverterHelper _helper;

        public BinarySerializer(IBinarySerializer serializer, IBinaryDeserializer deserializer, IList<BinaryConverter> customConverters = null)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _helper = new BitConverterHelper(customConverters);
        }

        public ISerializeResult Serialize<TData>(TData data)
        {
            var context = BinaryCache.GetOrAddContext(typeof(TData), _helper);
            return _serializer.Serialize(data, context);
        }

        public IDeserializeResult<TId, TData> Deserialize<TId, TData>(in ReadOnlySequence<byte> sequence) where TId : struct where TData : new()
        {
            var context = BinaryCache.GetOrAddContext(typeof(TData), _helper);
            return _deserializer.Deserialize<TId, TData>(sequence, context);
        }

        public async Task<IDeserializeResult<TId, TData>> DeserializeAsync<TId, TData>(PipeReader pipeReader, CancellationToken token = default) where TId : struct where TData : new()
        {
            IDeserializeResult<TId, TData> result;

            var context = BinaryCache.GetOrAddContext(typeof(TData), _helper);
            var metaReadResult = await pipeReader.ReadLengthAsync(context.ReflectionData.MetaLength, token);

            if (context.ReflectionData.LengthProperty == null)
            {
                var sequence = metaReadResult.Slice(context.ReflectionData.MetaLength);
                result = Deserialize<TId, TData>(sequence);
                pipeReader.Consume(sequence.GetPosition(context.ReflectionData.MetaLength));
            }
            else
            {
                var lengthAttribute = context.ReflectionData.LengthProperty.Attribute;
                var lengthSequence = metaReadResult.Slice(lengthAttribute.Length, lengthAttribute.Index);
                var lengthValue = context.BitConverterHelper.ConvertFromBytes(lengthSequence, context.ReflectionData.LengthProperty.Type, lengthAttribute.Reverse);
                var totalLength = context.ReflectionData.MetaLength + (lengthValue is int length ? length : Convert.ToInt32(lengthValue));

                ReadOnlySequence<byte> sequence;

                if (metaReadResult.Buffer.Length >= totalLength)
                    sequence = metaReadResult.Slice(totalLength);
                else
                {
                    pipeReader.Examine(metaReadResult.Buffer.Start, metaReadResult.Buffer.GetPosition(context.ReflectionData.MetaLength));
                    var totalReadResult = await pipeReader.ReadLengthAsync(totalLength, token);
                    sequence = totalReadResult.Slice(totalLength);
                }

                result = Deserialize<TId, TData>(sequence);
                pipeReader.Consume(sequence.GetPosition(totalLength));
            }

            return result;
        }
    }
}