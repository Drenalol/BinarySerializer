using System;

namespace Drenalol.Binary.Infrastracture
{
    public interface ISerializeResult : IDisposable
    {
        int Length { get; }
        ReadOnlyMemory<byte> Result { get; }
    }
}