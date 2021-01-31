using System;
using Drenalol.Binary.Helpers;

namespace Drenalol.Binary
{
    public class BinarySerializerContext
    {
        public readonly BitConverterHelper BitConverterHelper;
        public readonly ReflectionData ReflectionData;

        public BinarySerializerContext(BitConverterHelper helper, Type type)
        {
            BitConverterHelper = helper;
            ReflectionData = new ReflectionData(type);
        }
    }
}