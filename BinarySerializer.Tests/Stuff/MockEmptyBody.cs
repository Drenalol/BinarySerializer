using Drenalol.Binary.Attributes;

namespace Drenalol.BinSerializer.Tests.Stuff
{
    public class MockNoIdEmptyBody
    {
        [BinaryData(0, 2, BinaryDataType.Length)]
        public ushort Length { get; set; }

        [BinaryData(2, BinaryDataType = BinaryDataType.Body)]
        public string Empty { get; set; }
    }
}