using System;

namespace Drenalol.Binary.Infrastracture
{
    public interface ISerializeResult : IDisposable
    {
        int Length { get; }
        byte[] BytesResult { get; }
        ReadOnlyMemory<byte> MemoryResult { get; }
    }
}