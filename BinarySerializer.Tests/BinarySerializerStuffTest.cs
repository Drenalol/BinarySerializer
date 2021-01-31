using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Drenalol.Binary;
using Drenalol.Binary.Attributes;
using Drenalol.Binary.Converters;
using Drenalol.Binary.Exceptions;
using Drenalol.Binary.Helpers;
using Drenalol.Binary.Infrastracture.Implementations;
using Drenalol.BinSerializer.Tests.Stuff;
using NUnit.Framework;

namespace Drenalol.BinSerializer.Tests
{
    public class BinarySerializerStuffTest
    {
        public static IList<BinaryConverter> Converters = new List<BinaryConverter>
        {
            new BinaryUtf8StringConverter(),
            new BinaryGuidConverter(),
            new BinaryDateTimeConverter()
        };
        
        public static void RegisterConverters(BitConverterHelper bitConverterHelper)
        {
            bitConverterHelper.RegisterConverter(new BinaryUtf8StringConverter());
            bitConverterHelper.RegisterConverter(new BinaryGuidConverter());
            bitConverterHelper.RegisterConverter(new BinaryDateTimeConverter());
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ComposeAndBodyAttribute<T>
        {
            [BinaryData(1, BinaryDataType = BinaryDataType.Compose)]
            public T T1 { get; set; }

            [BinaryData(2, BinaryDataType = BinaryDataType.Body)]
            public string T2 { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DuplicateComposeAttribute<T>
        {
            [BinaryData(1, BinaryDataType = BinaryDataType.Compose)]
            public T T1 { get; set; }

            [BinaryData(2, BinaryDataType = BinaryDataType.Compose)]
            public T T2 { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DoesNotHaveAny
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DoesNotHaveBodyAttribute
        {
            [BinaryData(0, 1, BinaryDataType = BinaryDataType.Id)]
            public int Key { get; set; }

            [BinaryData(1, 2, BinaryDataType = BinaryDataType.Length)]
            public int Length { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DoesNotHaveBodyLengthAttribute
        {
            [BinaryData(0, 1, BinaryDataType = BinaryDataType.Id)]
            public int Key { get; set; }

            [BinaryData(1, 2, BinaryDataType = BinaryDataType.Body)]
            public int Body { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class KeyDoesNotHaveSetter
        {
            [BinaryData(0, 1, BinaryDataType = BinaryDataType.Id)]
            public int Key { get; }

            [BinaryData(1, 2, BinaryDataType = BinaryDataType.Length)]
            public int Length { get; set; }

            [BinaryData(3, 2, BinaryDataType = BinaryDataType.Body)]
            public int Body { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class MetaDataNotHaveSetter
        {
            [BinaryData(0, 4)]
            public int Meta { get; }
        }

        [Test]
        public void DoesNotHaveAnyErrorTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(DoesNotHaveAny), new BitConverterHelper(null)));
        }

        [Test]
        public void DoesNotHaveBodyAttributeErrorTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(DoesNotHaveBodyAttribute), new BitConverterHelper(null)));
        }

        [Test]
        public void MetaDataNotHaveSetterErrorTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(MetaDataNotHaveSetter), new BitConverterHelper(null)));
        }

        [Test]
        public void DoesNotHaveBodyLengthAttributeErrorTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(DoesNotHaveBodyLengthAttribute), new BitConverterHelper(null)));
        }

        [Test]
        public void KeyDoesNotHaveSetterErrorTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(KeyDoesNotHaveSetter), new BitConverterHelper(null)));
        }

        [Test]
        public void DuplicateComposeAttributeErrorTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(DuplicateComposeAttribute<MockOnlyMetaData>), new BitConverterHelper(null)));
        }

        [Test]
        public void BodyAndComposeAttributeAtSameTimeTest()
        {
            Assert.Catch(typeof(BinaryException), () => BinaryCache.GetOrAddContext(typeof(ComposeAndBodyAttribute<MockOnlyMetaData>), new BitConverterHelper(null)));
        }

        [TestCase(10000, false)]
        [TestCase(10000, true)]
        public async Task AttributeMockSerializeDeserializeTest(int count, bool useParallel)
        {
            var binarySerializer = new BinarySerializer(new StandartBinarySerializer(), new StandartBinaryDeserializer(), Converters);
            
            var mock = new AttributeMockSerialize
            {
                Id = TestContext.CurrentContext.Random.NextUInt(),
                DateTime = DateTime.Now.AddSeconds(TestContext.CurrentContext.Random.NextUInt()),
                LongNumbers = TestContext.CurrentContext.Random.NextULong(),
                IntNumbers = TestContext.CurrentContext.Random.NextUInt()
            };

            mock.BuildBody();

            var enumerable = Enumerable.Range(0, count);

            var tasks = (useParallel ? enumerable.AsParallel().Select(Selector) : enumerable.Select(Selector)).ToArray();

            await Task.WhenAll(tasks);

            Task Selector(int i) =>
                Task.Run(() =>
                {
                    var serialize = binarySerializer.Serialize(mock);
                    _ = binarySerializer.Deserialize<uint, AttributeMockSerialize>(new ReadOnlySequence<byte>(serialize.Result));
                });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BaseConvertersTest(bool reverse)
        {
            var helper = new BitConverterHelper(Converters);

            var str = "Hello my friend";
            var stringResult = helper.ConvertToBytes(str, typeof(string), reverse);
            var stringResultBack = helper.ConvertFromBytes(new ReadOnlySequence<byte>(stringResult), typeof(string), reverse);
            Assert.AreEqual(str, stringResultBack);

            var datetime = DateTime.Now;
            var dateTimeResult = helper.ConvertToBytes(datetime, typeof(DateTime), reverse);
            var dateTimeResultBack = helper.ConvertFromBytes(new ReadOnlySequence<byte>(dateTimeResult), typeof(DateTime), reverse);
            Assert.AreEqual(datetime, dateTimeResultBack);

            var guid = Guid.NewGuid();
            var guidResult = helper.ConvertToBytes(guid, typeof(Guid), reverse);
            var guidResultBack = helper.ConvertFromBytes(new ReadOnlySequence<byte>(guidResult), typeof(Guid), reverse);
            Assert.AreEqual(guid, guidResultBack);
        }
    }
}