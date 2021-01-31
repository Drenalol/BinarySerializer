using System;
using Drenalol.Binary.Models;

namespace Drenalol.Binary.Infrastracture.Implementations
{
    public class StandartSerializeResult : ISerializeResult
    {
        public int Length { get; }
        public byte[] BytesResult { get; private set; }
        public ReadOnlyMemory<byte> MemoryResult => new ReadOnlyMemory<byte>(BytesResult, 0, Length);
        
        private readonly StandartSerializeResult _composeSerializeResult;
        
        public StandartSerializeResult(int realLength, StandartSerializeResult composeSerializeResult)
        {
            BytesResult = new byte[realLength];
            Length = realLength;
            _composeSerializeResult = composeSerializeResult;
        }

        public void Dispose()
        {
            BytesResult = null;
            _composeSerializeResult?.Dispose();
        }
    }
}