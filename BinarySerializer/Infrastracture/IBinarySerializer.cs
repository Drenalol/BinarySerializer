namespace Drenalol.Binary.Infrastracture
{
    public interface IBinarySerializer
    {
        ISerializeResult Serialize(object data, BinarySerializerContext context);
    }
}