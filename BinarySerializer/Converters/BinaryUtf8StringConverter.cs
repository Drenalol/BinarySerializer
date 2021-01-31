using System;
using System.Text;

namespace Drenalol.Binary.Converters
{
    /// <summary>
    /// String converter to byte array and vice versa.
    /// </summary>
    public class BinaryUtf8StringConverter : BinaryConverter<string>
    {
        public override byte[] Convert(string input) => Encoding.UTF8.GetBytes(input);
        public override string ConvertBack(ReadOnlySpan<byte> input) => Encoding.UTF8.GetString(input);
    }
}