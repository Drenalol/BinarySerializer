using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Drenalol.Binary;
using Drenalol.Binary.Exceptions;
using Drenalol.Binary.Helpers;
using Drenalol.Binary.Infrastracture.Implementations;
using Drenalol.BinSerializer.Tests.Stuff;
using NUnit.Framework;

namespace Drenalol.BinSerializer.Tests
{
    public class BinarySerializerTest
    {
        [Test]
        public void SerializeWithPoolDeserializeTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            var ethalon = Mock.Default();
            const int ethalonHeaderLength = 270;

            var serialize = binarySerializer.Serialize(ethalon);
            Assert.IsTrue(serialize.Length == ethalon.Size + ethalonHeaderLength);
            var deserialize = binarySerializer.Deserialize<long, Mock>(new ReadOnlySequence<byte>(serialize.BytesResult));
            Assert.IsTrue(ethalon.Equals(deserialize.Result));
        }

        [Test]
        public async Task SerializeWithPoolDeserializeFromPipeReaderTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            var ethalon = Mock.Default();
            const int ethalonHeaderLength = 270;

            var serialize = binarySerializer.Serialize(ethalon);
            Assert.IsTrue(serialize.Length == ethalon.Size + ethalonHeaderLength);
            var deserialize = await binarySerializer.DeserializeAsync<long, Mock>(PipeReader.Create(new MemoryStream(serialize.BytesResult)), CancellationToken.None);
            Assert.IsTrue(ethalon.Equals(deserialize.Result));
            serialize.Dispose();
        }

        [Test]
        public void NotFoundConverterExceptionTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer());
            var mock = Mock.Default();
            Assert.Catch<BinaryException>(() => binarySerializer.Serialize(mock));
        }

        [TestCase(true, 1, false)]
        [TestCase('c', 2, false)]
        [TestCase(1234.0, 8, false)]
        [TestCase((short) 1234, 2, false)]
        [TestCase(1234, 4, false)]
        [TestCase(1234L, 8, true)]
        [TestCase(1234F, 4, false)]
        [TestCase((ushort) 1234, 2, false)]
        [TestCase(1234U, 4, true)]
        [TestCase(1234UL, 8, true)]
        public void BitConverterToBytesTest(object obj, int expected, bool reverse)
        {
            var converter = new BitConverterHelper(null);
            Assert.That(converter.ConvertToBytes(obj, obj.GetType()).Length == expected, "converter.ConvertToBytes(obj, obj.GetType()).Length == expected");
        }

        [TestCase(new byte[] {25, 75}, typeof(short), false)]
        [TestCase(new byte[] {0, 1, 2, 5}, typeof(int), false)]
        [TestCase(new byte[] {0, 1, 2, 3, 4, 5, 6, 7}, typeof(long), false)]
        public void BitConverterFromBytesTest(byte[] bytes, Type type, bool reverse)
        {
            var converter = new BitConverterHelper(null);
            Assert.That(converter.ConvertFromBytes(new ReadOnlySequence<byte>(bytes), type, reverse).GetType() == type, "converter.ConvertFromBytes(bytes, type, reverse).GetType() == type");
        }

        [Test]
        public void SerializeStandartRecursiveComposeTypeTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>>.Create(
                RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>.Create(
                    RecursiveMock<RecursiveMock<MockOnlyMetaData>>.Create(
                        RecursiveMock<MockOnlyMetaData>.Create(
                            new MockOnlyMetaData()
                        )
                    )
                )
            );
            var result = binarySerializer.Serialize(mock);
            result.Dispose();
        }
        
        [Test]
        public void SerializeWithPoolRecursiveComposeTypeTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>>.Create(
                RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>.Create(
                    RecursiveMock<RecursiveMock<MockOnlyMetaData>>.Create(
                        RecursiveMock<MockOnlyMetaData>.Create(
                            new MockOnlyMetaData()
                        )
                    )
                )
            );
            var result = binarySerializer.Serialize(mock);
            result.Dispose();
        }

        [Test]
        public void SerializeStandartRecursiveStructComposeTypeTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMockStruct<RecursiveMockStruct<RecursiveMockStruct<RecursiveMockStruct<MockOnlyMetaData>>>>.Create(
                RecursiveMockStruct<RecursiveMockStruct<RecursiveMockStruct<MockOnlyMetaData>>>.Create(
                    RecursiveMockStruct<RecursiveMockStruct<MockOnlyMetaData>>.Create(
                        RecursiveMockStruct<MockOnlyMetaData>.Create(
                            new MockOnlyMetaData()
                        )
                    )
                )
            );
            var result = binarySerializer.Serialize(mock);
            result.Dispose();
        }
        
        [Test]
        public void SerializeWithPoolRecursiveStructComposeTypeTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMockStruct<RecursiveMockStruct<RecursiveMockStruct<RecursiveMockStruct<MockOnlyMetaData>>>>.Create(
                RecursiveMockStruct<RecursiveMockStruct<RecursiveMockStruct<MockOnlyMetaData>>>.Create(
                    RecursiveMockStruct<RecursiveMockStruct<MockOnlyMetaData>>.Create(
                        RecursiveMockStruct<MockOnlyMetaData>.Create(
                            new MockOnlyMetaData()
                        )
                    )
                )
            );
            var result = binarySerializer.Serialize(mock);
            result.Dispose();
        }

        [Test]
        public void SerializeStandartDeserializeRecursiveComposeTypeTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>.Create(
                RecursiveMock<RecursiveMock<MockOnlyMetaData>>.Create(
                    RecursiveMock<MockOnlyMetaData>.Create(
                        new MockOnlyMetaData()
                    )
                )
            );
            
            var serialize = binarySerializer.Serialize(mock);
            var data = binarySerializer.Deserialize<long, RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>>(new ReadOnlySequence<byte>(serialize.BytesResult));
            Assert.NotNull(data);
        }
        
        [Test]
        public async Task SerializeWithPoolDeserializeFromPipeRecursiveComposeTypeTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>.Create(
                RecursiveMock<RecursiveMock<MockOnlyMetaData>>.Create(
                    RecursiveMock<MockOnlyMetaData>.Create(
                        new MockOnlyMetaData()
                    )
                )
            );
            
            var serialize = binarySerializer.Serialize(mock);
            var pipe = PipeReader.Create(new MemoryStream(serialize.BytesResult));
            var data = await binarySerializer.DeserializeAsync<long, RecursiveMock<RecursiveMock<RecursiveMock<MockOnlyMetaData>>>>(pipe, CancellationToken.None);
            Assert.NotNull(data);
        }

        [Test]
        public void SerializeStandartDeserializeRecursiveComposeValueTypeTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<int>.Create(1337);
            var serialize = binarySerializer.Serialize(mock);
            var data = binarySerializer.Deserialize<long, RecursiveMock<int>>(new ReadOnlySequence<byte>(serialize.BytesResult));
            
            Assert.NotNull(data);
            Assert.AreEqual(mock.Id, data.Identifier);
            Assert.AreEqual(mock.Id, data.Result.Id);
            Assert.AreEqual(mock.Size, data.Result.Size);
            Assert.AreEqual(mock.Data, data.Result.Data);
        }
        
        [Test]
        public async Task SerializeWithPoolDeserializeFromPipeRecursiveComposeValueTypeTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<int>.Create(1337);
            var serialize = binarySerializer.Serialize(mock);
            var pipe = PipeReader.Create(new MemoryStream(serialize.BytesResult));
            var data = await binarySerializer.DeserializeAsync<long, RecursiveMock<int>>(pipe, CancellationToken.None);
            
            Assert.NotNull(data);
        }

        [Test]
        public void SerializeWithPoolSpeedTest()
        {
            var binarySerializer = new BinarySerializer(new ArrayPoolBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>>.Create(
                RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>.Create(
                    RecursiveMock<RecursiveMock<RecursiveMock<int>>>.Create(
                        RecursiveMock<RecursiveMock<int>>.Create(
                            RecursiveMock<int>.Create(int.MaxValue
                            )
                        )
                    )
                )
            );
            var sw = Stopwatch.StartNew();

            for (var i = 0; i < 10000; i++)
            {
                _ = binarySerializer.Serialize(mock);
            }

            sw.Stop();
            Assert.Less(sw.ElapsedMilliseconds, 300);
            TestContext.WriteLine($"Serialize: {sw.Elapsed.ToString()}");
        }
        
        [Test]
        public void SerializeStandartSpeedTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>>.Create(
                RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>.Create(
                    RecursiveMock<RecursiveMock<RecursiveMock<int>>>.Create(
                        RecursiveMock<RecursiveMock<int>>.Create(
                            RecursiveMock<int>.Create(int.MaxValue
                            )
                        )
                    )
                )
            );
            var sw = Stopwatch.StartNew();

            for (var i = 0; i < 10000; i++)
            {
                _ = binarySerializer.Serialize(mock);
            }

            sw.Stop();
            Assert.Less(sw.ElapsedMilliseconds, 300);
            TestContext.WriteLine($"Serialize: {sw.Elapsed.ToString()}");
        }
        
        [Test]
        public async Task DeserializeFromPipeSpeedTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>>.Create(
                RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>.Create(
                    RecursiveMock<RecursiveMock<RecursiveMock<int>>>.Create(
                        RecursiveMock<RecursiveMock<int>>.Create(
                            RecursiveMock<int>.Create(int.MaxValue
                            )
                        )
                    )
                )
            );
            
            var serialize = binarySerializer.Serialize(mock);

            var sw = Stopwatch.StartNew();
            
            for (var i = 0; i < 10000; i++)
            {
                var pipe = PipeReader.Create(new MemoryStream(serialize.BytesResult));
                sw.Start();
                var data = await binarySerializer.DeserializeAsync<long, RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>>>(pipe, CancellationToken.None);
                sw.Stop();
                Assert.NotNull(data);
            }

            Assert.Less(sw.ElapsedMilliseconds, 300);
            TestContext.WriteLine($"Deserialize: {sw.Elapsed.ToString()}");
        }
        
        [Test]
        public void DeserializeFromBytesSpeedTest()
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), BinarySerializerStuffTest.Converters);
            
            var mock = RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>>.Create(
                RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>.Create(
                    RecursiveMock<RecursiveMock<RecursiveMock<int>>>.Create(
                        RecursiveMock<RecursiveMock<int>>.Create(
                            RecursiveMock<int>.Create(int.MaxValue
                            )
                        )
                    )
                )
            );
            
            var serialize = binarySerializer.Serialize(mock);

            var sw = Stopwatch.StartNew();
            
            for (var i = 0; i < 10000; i++)
            {
                var sequence = new ReadOnlySequence<byte>(serialize.BytesResult);
                sw.Start();
                var data = binarySerializer.Deserialize<long, RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<RecursiveMock<int>>>>>>(sequence);
                sw.Stop();
                Assert.NotNull(data);
            }

            Assert.Less(sw.ElapsedMilliseconds, 300);
            TestContext.WriteLine($"Deserialize: {sw.Elapsed.ToString()}");
        }
    }
}