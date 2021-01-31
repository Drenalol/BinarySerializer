using System;

namespace Drenalol.Binary.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public enum BinaryDataType
    {
        /// <summary>
        /// 
        /// </summary>
        MetaData,

        /// <summary>
        /// 
        /// </summary>
        Id,

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use BinaryDataType.Length instead", true)]
        BodyLength,

        /// <summary>
        /// 
        /// </summary>
        Length,

        /// <summary>
        /// 
        /// </summary>
        Body,

        /// <summary>
        /// 
        /// </summary>
        Compose
    }
}