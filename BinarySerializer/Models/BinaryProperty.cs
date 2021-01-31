using System;
using System.Reflection;
using Drenalol.Binary.Attributes;

namespace Drenalol.Binary.Models
{
    internal class BinaryProperty : BinaryMember
    {
        private readonly PropertyInfo _propertyInfo;

        public BinaryProperty(PropertyInfo propertyInfo, BinaryDataAttribute attribute, Type accessorType)
            : base(propertyInfo.PropertyType, attribute, accessorType)
        {
            _propertyInfo = propertyInfo;
        }

        public override object Get(object input) => _propertyInfo.GetValue(input);

        public override object Set(object input, object value)
        {
            _propertyInfo.SetValue(input, value);
            return input;
        }
    }
}