using System;
using System.Collections.Concurrent;
using Drenalol.Binary.Helpers;

namespace Drenalol.Binary
{
    public static class BinaryCache
    {
        private static readonly ConcurrentDictionary<Type, BinarySerializerContext> Cache = new ConcurrentDictionary<Type, BinarySerializerContext>();
        
        public static BinarySerializerContext GetOrAddContext(Type typeData, BitConverterHelper helper) => Cache.GetOrAdd(typeData, type => new BinarySerializerContext(helper, type));
    }
}