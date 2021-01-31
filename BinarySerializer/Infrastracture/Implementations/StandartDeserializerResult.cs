namespace Drenalol.Binary.Infrastracture.Implementations
{
    public class StandartDeserializerResult<TId, TData> : IDeserializeResult<TId, TData> where TId : struct
    {
        public TId? Identifier { get; }
        public TData Result { get; }

        public StandartDeserializerResult(TId? identifier, TData result)
        {
            Identifier = identifier;
            Result = result;
        }

        public static implicit operator StandartDeserializerResult<TId, TData>(StandartBinaryDeserializer.InternalDeserializeResult result)
        {
            var data = (TData) result.Data;
            return new StandartDeserializerResult<TId, TData>((TId?) result.Id, data);
        }
    }
}