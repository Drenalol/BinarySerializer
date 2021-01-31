using System;

namespace Drenalol.Binary.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BinaryDataAttribute : Attribute
    {
        /// <summary>
        /// Property position in Byte Array.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Property length in Byte Array. If BinaryDataType set to BinaryDataType.Body is ignored. Overwritten by the serializer.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Optional. Reverses the sequence of the elements in the serialized Byte Array.
        /// <para>Used for cases where the receiving side uses a different endianness.</para>
        /// </summary>
        public bool? Reverse { get; set; }

        /// <summary>
        /// Sets the property type for the serializer.
        /// </summary>
        [Obsolete]
        public Type Type { get; set; }
        
        /// <summary>
        /// Sets the serialization rule for this property.
        /// </summary>
        public BinaryDataType BinaryDataType { get; set; }

        public BinaryDataAttribute(int index, int length = 0, BinaryDataType binaryDataType = BinaryDataType.MetaData)
        {
            Index = index;
            Length = length;
            BinaryDataType = binaryDataType;
        }

        public override string ToString() => $"{Index.ToString()}, {Length.ToString()}, {BinaryDataType.ToString()}";
    }
}