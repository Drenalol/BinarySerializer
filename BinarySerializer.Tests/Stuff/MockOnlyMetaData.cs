using Drenalol.Binary.Attributes;

namespace Drenalol.BinSerializer.Tests.Stuff
{
    public class MockOnlyMetaData
    {
        [BinaryData(0, 4)]
        public int Test { get; set; }

        [BinaryData(4, 8)]
        public long Long { get; set; }

        public MockOnlyMetaData()
        {
            Test = 5555;
            Long = 12312312;
        }
    }
}