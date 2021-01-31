using System;
using System.Reflection;
using Drenalol.Binary.Attributes;

namespace Drenalol.Binary.Models
{
    internal class BinaryField : BinaryMember
    {
        private readonly FieldInfo _fieldInfo;

        public BinaryField(FieldInfo fieldInfo, BinaryDataAttribute attribute, Type accessorType)
            : base(fieldInfo.FieldType, attribute, accessorType)
        {
            _fieldInfo = fieldInfo;
        }

        public override object Get(object input) => _fieldInfo.GetValue(input);

        public override object Set(object input, object value)
        {
            _fieldInfo.SetValue(input, value);
            return input;
        }
    }
}