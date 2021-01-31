using System;
using System.Buffers;

namespace Drenalol.Binary.Infrastracture.Implementations
{
    public class ArrayPoolSerializeResult : ISerializeResult
    {
        public int Length { get; }
        public byte[] BytesResult => BytesRented[..Length];
        public ReadOnlyMemory<byte> MemoryResult => new ReadOnlyMemory<byte>(BytesRented, 0, Length);
        public byte[] BytesRented { get; }
        
        private readonly ArrayPoolSerializeResult _composeArrayPoolSerializeResult;
        private readonly ArrayPool<byte> _arrayPool;

        public ArrayPoolSerializeResult(int realLength, ArrayPoolSerializeResult composeArrayPoolSerializeResult, ArrayPool<byte> arrayPool)
        {
            Length = realLength;
            _composeArrayPoolSerializeResult = composeArrayPoolSerializeResult;
            _arrayPool = arrayPool;
            BytesRented = _arrayPool.Rent(realLength);
        }
        
        public void Dispose()
        {
            _arrayPool.Return(BytesRented, true);
            _composeArrayPoolSerializeResult?.Dispose();
        }
    }
}