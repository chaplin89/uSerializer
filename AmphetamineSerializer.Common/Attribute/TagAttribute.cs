using System;

namespace AmphetamineSerializer.Common.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public enum ParameterRole
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
        public TagAttribute(ParameterRole type)
        {
            Type = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public ParameterRole Type { get; private set; }
    }
}
