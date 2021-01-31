using System.Buffers;

namespace Drenalol.Binary.Infrastracture
{
    public interface IBinaryDeserializer
    {
        IDeserializeResult<TId, TData> Deserialize<TId, TData>(in ReadOnlySequence<byte> sequence, BinarySerializerContext context) where TId : struct where TData : new();
    }
}