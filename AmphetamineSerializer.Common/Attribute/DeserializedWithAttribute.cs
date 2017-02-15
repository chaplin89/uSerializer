using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Used for specify an object that manage the serialization in order to completely 
    /// skip the default dynamic generation of assemblies while mantaining a consistent syntax.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SerializedWithAttribute : Attribute
    {
        /// <summary>
        /// Type that manage the serialization.
        /// </summary>
        public Type SerializatorType { get; }

        /// <summary>
        /// Construct the attribute.
        /// </summary>
        /// <param name="serializatorType">Type that manage the serialization</param>
        public SerializedWithAttribute(Type serializatorType)
        {
            SerializatorType = serializatorType;
        }
    }
}