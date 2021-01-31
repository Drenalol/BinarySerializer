using System;
using Drenalol.Binary.Attributes;
using NUnit.Framework;

namespace Drenalol.BinSerializer.Tests.Stuff
{
    public class AttributeMockSerialize
    {
        [BinaryData(0, 4, BinaryDataType = BinaryDataType.Id)]
        public uint Id { get; set; }

        [BinaryData(4, 4, BinaryDataType = BinaryDataType.Length)]
        public uint Size { get; set; }

        [BinaryData(8, 8)]
        public ulong LongNumbers { get; set; }

        [BinaryData(16, 4)]
        public uint IntNumbers { get; set; }

        [BinaryData(20, 8)]
        public DateTime DateTime { get; set; }

        [BinaryData(28, 60)]
        public string NotFull { get; set; }

        [BinaryData(88, BinaryDataType = BinaryDataType.Body)]
        public byte[] Body { get; set; }

        public void BuildBody()
        {
            NotFull = "TestStringAndNot60Length";
            Body = new byte[TestContext.CurrentContext.Random.Next(10, 1024)];
            TestContext.CurrentContext.Random.NextBytes(Body);
            Size = (uint) Body.Length;
        }
    }
}