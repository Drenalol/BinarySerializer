using Drenalol.Binary.Models;

namespace Drenalol.Binary.Infrastracture
{
    public interface IBinarySerializer
    {
        ISerializeResult Serialize<TData>(TData data, BinarySerializerContext context);
    }
}