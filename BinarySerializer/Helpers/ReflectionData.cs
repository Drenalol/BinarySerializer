using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Drenalol.Binary.Attributes;
using Drenalol.Binary.Exceptions;
using Drenalol.Binary.Models;

namespace Drenalol.Binary.Helpers
{
    public class ReflectionData
    {
        public readonly IReadOnlyList<BinaryMember> Properties;
        public readonly int MetaLength;
        public readonly BinaryMember IdProperty;
        public readonly BinaryMember LengthProperty;
        public readonly BinaryMember BodyProperty;
        public readonly BinaryMember ComposeProperty;

        public ReflectionData(Type typeData)
        {
            EnsureTypeHasRequiredAttributes(typeData);
            Properties = GetTypeProperties(typeData).ToList();
            MetaLength = Properties.Sum(p => p.Attribute.Length);
            IdProperty = Properties.SingleOrDefault(p => p.Attribute.BinaryDataType == BinaryDataType.Id);
            LengthProperty = Properties.SingleOrDefault(p => p.Attribute.BinaryDataType == BinaryDataType.Length);
            BodyProperty = Properties.SingleOrDefault(p => p.Attribute.BinaryDataType == BinaryDataType.Body);
            ComposeProperty = Properties.SingleOrDefault(p => p.Attribute.BinaryDataType == BinaryDataType.Compose);
        }

        private static IEnumerable<BinaryMember> GetTypeProperties(Type typeData)
            => (from item in GetMembers(typeData)
                    let attribute = GetTcpDataAttribute(item.Member)
                    where attribute != null
                    select item.IsProperty
                        ? (BinaryMember) new BinaryProperty((PropertyInfo) item.Member, attribute, typeData)
                        : new BinaryField((FieldInfo) item.Member, attribute, typeData))
                .OrderBy(member => member.Attribute.Index);

        private static void EnsureTypeHasRequiredAttributes(Type typeData)
        {
            var members = GetMembers(typeData).ToArray();

            var key = members.Where(item => GetTcpDataAttribute(item.Member).BinaryDataType == BinaryDataType.Id).ToList();

            if (key.Count > 1)
                throw BinaryException.AttributeDuplicate(typeData.ToString(), nameof(BinaryDataType.Id));

            if (key.Count == 1 && key.Single() is {IsProperty: true} internalMember1 && !CanReadWrite((PropertyInfo) internalMember1.Member))
                throw BinaryException.PropertyCanReadWrite(typeData.ToString(), nameof(BinaryDataType.Id));

            var body = members.Where(item => GetTcpDataAttribute(item.Member).BinaryDataType == BinaryDataType.Body).ToList();

            if (body.Count > 1)
                throw BinaryException.AttributeDuplicate(typeData.ToString(), nameof(BinaryDataType.Body));

            if (body.Count == 1 && body.Single() is {IsProperty: true} internalMember2 && !CanReadWrite((PropertyInfo) internalMember2.Member))
                throw BinaryException.PropertyCanReadWrite(typeData.ToString(), nameof(BinaryDataType.Body));

            var compose = members.Where(item => GetTcpDataAttribute(item.Member).BinaryDataType == BinaryDataType.Compose).ToList();

            if (compose.Count > 1)
                throw BinaryException.AttributeDuplicate(typeData.ToString(), nameof(BinaryDataType.Compose));

            if (body.Count == 1 && compose.Count == 1)
                throw BinaryException.AttributeBodyAndComposeViolated(typeData.ToString());

            var length = members.Where(item => GetTcpDataAttribute(item.Member).BinaryDataType == BinaryDataType.Length).ToList();

            if (length.Count > 1)
                throw BinaryException.AttributeDuplicate(typeData.ToString(), nameof(BinaryDataType.Length));

            if (body.Count == 1)
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (length.Count == 0)
                    throw BinaryException.AttributeLengthRequired(typeData.ToString(), nameof(BinaryDataType.Body));

                if (length.Count == 1 && length.Single() is {IsProperty: true} internalMember3 && !CanReadWrite((PropertyInfo) internalMember3.Member))
                    throw BinaryException.PropertyCanReadWrite(typeData.ToString(), nameof(BinaryDataType.Length));
            }
            else if (compose.Count == 1)
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (length.Count == 0)
                    throw BinaryException.AttributeLengthRequired(typeData.ToString(), nameof(BinaryDataType.Compose));

                if (length.Count == 1 && length.Single() is {IsProperty: true} internalMember4 && !CanReadWrite((PropertyInfo) internalMember4.Member))
                    throw BinaryException.PropertyCanReadWrite(typeData.ToString(), nameof(BinaryDataType.Length));
            }
            else if (length.Count == 1)
                throw BinaryException.AttributeRequiredWithLength(typeData.ToString());

            var metaData = members.Where(item => GetTcpDataAttribute(item.Member).BinaryDataType == BinaryDataType.MetaData).ToList();

            if (key.Count == 0 && length.Count == 0 && body.Count == 0 && metaData.Count == 0)
                throw BinaryException.AttributesRequired(typeData.ToString());

            foreach (var item in metaData.Where(item => item.IsProperty && !CanReadWrite((PropertyInfo) item.Member)))
                throw BinaryException.PropertyCanReadWrite(typeData.ToString(), nameof(BinaryDataType.MetaData), GetTcpDataAttribute(item.Member).Index.ToString());

            static bool CanReadWrite(PropertyInfo property) => property.CanRead && property.CanWrite;
        }

        private static IEnumerable<InternalReflectionMember> GetMembers(Type type)
            => type.GetProperties().Select(member => new InternalReflectionMember
            {
                IsProperty = true,
                Member = member,
                Type = member.PropertyType
            }).Union(type.GetFields().Select(member => new InternalReflectionMember
            {
                IsProperty = false,
                Member = member,
                Type = member.FieldType
            }));

        private static BinaryDataAttribute GetTcpDataAttribute(ICustomAttributeProvider item)
            => item
                .GetCustomAttributes(true)
                .OfType<BinaryDataAttribute>()
                .SingleOrDefault();
        
        internal class InternalReflectionMember
        {
            public bool IsProperty;
            public MemberInfo Member;
            public Type Type;
        }
    }
}