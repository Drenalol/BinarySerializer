using System;

namespace Drenalol.Binary.Converters
{
    /// <summary>
    /// Guid converter to byte array and vice versa.
    /// </summary>
    public class BinaryGuidConverter : BinaryConverter<Guid>
    {
        public override byte[] Convert(Guid input) => input.ToByteArray();
        public override Guid ConvertBack(ReadOnlySpan<byte> input) => new Guid(input);
    }
}