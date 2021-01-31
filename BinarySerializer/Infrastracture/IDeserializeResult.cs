namespace Drenalol.Binary.Infrastracture
{
    public interface IDeserializeResult<TId, out TData> where TId : struct
    {
        TId? Identifier { get; }
        TData Result { get; }
    }
}