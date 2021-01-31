using Drenalol.Binary.Attributes;

namespace Drenalol.BinSerializer.Tests.Stuff
{
    public struct MockByteBody
    {
        [BinaryData(0, 4, BinaryDataType = BinaryDataType.Id)]
        public int Id { get; set; }
        
        [BinaryData(4, 4, BinaryDataType = BinaryDataType.Length)]
        public int Length { get; set; }
        
        [BinaryData(8, 1)]
        public byte TestByte { get; set; }
        
        [BinaryData(9, 2)]
        public byte[] TestByteArray { get; set; }
        
        [BinaryData(11, BinaryDataType = BinaryDataType.Body)]
        public string Body { get; set; }
    }
}