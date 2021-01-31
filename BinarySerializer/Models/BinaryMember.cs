using System;
using Drenalol.Binary.Attributes;

namespace Drenalol.Binary.Models
{
    public abstract class BinaryMember
    {
        public readonly BinaryDataAttribute Attribute;
        public readonly bool IsValueType;
        public readonly Type Type;
        public readonly bool IsPrimitive;

        protected BinaryMember(Type type, BinaryDataAttribute attribute, Type accessorType)
        {
            Attribute = attribute;
            IsValueType = accessorType.IsValueType;
            Type = type;
            IsPrimitive = type.IsPrimitive;
        }

        public abstract object Get(object input);
        public abstract object Set(object input, object value);
    }
}