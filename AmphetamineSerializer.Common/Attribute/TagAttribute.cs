using System;

namespace AmphetamineSerializer.Common.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// 
        /// </summary>
        MandatoryForward,

        /// <summary>
        /// 
        /// </summary>
        OptionalForward,

        /// <summary>
        /// 
        /// </summary>
        RootObject,

        /// <summary>
        /// 
        /// </summary>
        Auto
    }

    /// <summary>
    /// 
    /// </summary>
    public class TagAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public TagAttribute(ParameterType type)
        {
            Type = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public ParameterType Type { get; private set; }
    }
}
