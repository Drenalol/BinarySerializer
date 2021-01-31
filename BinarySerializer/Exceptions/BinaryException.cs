using System;
using Drenalol.Binary.Attributes;

namespace Drenalol.Binary.Exceptions
{
    /// <summary>
    /// Represents errors that occur during BinarySerializer process.
    /// </summary>
    public class BinaryException : Exception
    {
        private BinaryException(string message) : base(message)
        {
        }

        public static BinaryException ConverterError(string converterName) =>
            new BinaryException($"Converter {converterName} does not have generic type");

        public static BinaryException SerializerSequenceViolated() =>
            new BinaryException($"Sequence violated in {nameof(BinaryDataAttribute.Index)}");

        public static BinaryException SerializerLengthOutOfRange(string propertyName, string valueLength, string attributeLength) =>
            new BinaryException($"({propertyName}, {valueLength} bytes) is greater than attribute length {attributeLength} bytes");

        public static BinaryException PropertyArgumentIsNull(string propertyName) =>
            new BinaryException($"NULL value cannot be converted ({propertyName})");

        public static BinaryException PropertyCanReadWrite(string type, string attributeType, string attributeIndex = null) =>
            new BinaryException($"Set and Get keywords required for Serialization. Type: {type}, {nameof(BinaryDataType)}: {attributeType}, {(attributeType == nameof(BinaryDataType.MetaData) ? $"Index: {attributeIndex}" : null)}");

        public static BinaryException ConverterNotFoundType(string propertyName) =>
            new BinaryException($"Converter not found for {propertyName}");

        public static BinaryException ConverterUnknownError(string propertyName, string errorMessage) =>
            new BinaryException($"Error while trying convert data {propertyName}, error: {errorMessage}");

        public static BinaryException AttributesRequired(string type) =>
            new BinaryException($"{type} does not have any {nameof(BinaryDataAttribute)}");

        public static BinaryException AttributeLengthRequired(string type, string attribute) =>
            new BinaryException($"In {type} {nameof(BinaryDataType)}.{attribute} could not work without {nameof(BinaryDataType)}.{nameof(BinaryDataType.Length)}");

        public static BinaryException AttributeRequiredWithLength(string type) =>
            new BinaryException($"In {type} {nameof(BinaryDataType)}.{nameof(BinaryDataType.Length)} could not work without {nameof(BinaryDataType)}.{nameof(BinaryDataType.Body)} or {nameof(BinaryDataType)}.{nameof(BinaryDataType.Compose)}");

        public static BinaryException AttributeDuplicate(string type, string attributeType) =>
            new BinaryException($"{type} could not work with multiple {attributeType}");

        public static BinaryException SerializerBodyPropertyIsNull() =>
            new BinaryException($"Value of {nameof(BinaryDataType)}.{nameof(BinaryDataType.Body)} is Null");

        public static BinaryException SerializerComposePropertyIsNull() =>
            new BinaryException($"Value of {nameof(BinaryDataType)}.{nameof(BinaryDataType.Compose)} is Null");

        public static BinaryException AttributeBodyAndComposeViolated(string type) =>
            new BinaryException($"In {type} found {nameof(BinaryDataType)}.{nameof(BinaryDataType.Body)} and {nameof(BinaryDataType)}.{nameof(BinaryDataType.Compose)} at the same time");

        public static BinaryException SerializerPropertyMustHaveParameterlessCtor(Type type) =>
            new BinaryException($"In {type} must have at least one parameterless constructor");
    }
}