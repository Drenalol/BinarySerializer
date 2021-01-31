using Drenalol.Binary.Attributes;
using Newtonsoft.Json;

namespace Drenalol.BinSerializer.Tests.Stuff
{
    public struct RecursiveMockStruct<T>
    {
        [BinaryData(0, 8, BinaryDataType.Id)]
        [JsonIgnore]
        public long Id { get; set; }

        [BinaryData(8, 4, BinaryDataType.Length)]
        public int Size { get; set; }

        [BinaryData(12, 58)]
        public string FirstName { get; set; }

        [BinaryData(70, 50)]
        public string LastName { get; set; }

        [BinaryData(120, 50)]
        public string Email { get; set; }

        [BinaryData(170, 50)]
        public string Gender { get; set; }

        [BinaryData(220, 50)]
        public string IpAddress { get; set; }

        [BinaryData(270, BinaryDataType = BinaryDataType.Compose)]
        public T Data { get; set; }

        public static RecursiveMockStruct<T> Create(T data)
        {
            return new RecursiveMockStruct<T>
            {
                Id = 1337,
                Email = "amavin2@etsy.com",
                FirstName = "Adelina",
                LastName = "Mavin",
                Gender = "Female",
                IpAddress = "42.241.120.161",
                Data = data,
                Size = 0
            };
        }
    }
}