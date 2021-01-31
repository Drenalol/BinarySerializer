using System;
using Drenalol.Binary.Models;

namespace Drenalol.Binary.Infrastracture.Implementations
{
    public class StandartSerializeResult : ISerializeResult
    {
        public int Length { get; }
        public ReadOnlyMemory<byte> Result => new ReadOnlyMemory<byte>(Bytes, 0, Length);
        public byte[] Bytes { get; private set; }
        
        private readonly StandartSerializeResult _composeSerializeResult;
        
        public StandartSerializeResult(int realLength, StandartSerializeResult composeSerializeResult)
        {
            Bytes = new byte[realLength];
            Length = realLength;
            _composeSerializeResult = composeSerializeResult;
        }

        public void Dispose()
        {
            Bytes = null;
            _composeSerializeResult?.Dispose();
        }
    }
}