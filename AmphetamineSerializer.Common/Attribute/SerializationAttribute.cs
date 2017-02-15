using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Used for specify a method that handle the (de)serialization of a given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SerializationHandlerAttribute : Attribute
    {
        /// <summary>
        /// Managed type.
        /// </summary>
        public Type ContainedType { get; }

        /// <summary>
        /// Construct the attribute with a managed type.
        /// </summary>
        /// <param name="managedType">The managed type</param>
        public SerializationHandlerAttribute(Type managedType)
        {
            ContainedType = managedType;
        }
    }
}
