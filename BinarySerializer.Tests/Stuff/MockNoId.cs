using Drenalol.Binary.Attributes;

namespace Drenalol.BinSerializer.Tests.Stuff
{
    public class MockNoId
    {
        [BinaryData(0, 4, BinaryDataType.Length)]
        public int Size { get; set; }

        [BinaryData(4, BinaryDataType = BinaryDataType.Body)]
        public string Body { get; set; }
    }
}