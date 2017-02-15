using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Specify a custom builder for a given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BuildedWithAttribute : Attribute
    {
        /// <summary>
        /// Type that will manage the build process.
        /// </summary>
        public Type BuilderType { get; }

        /// <summary>
        /// Instantiate the attribute and configure the type.
        /// </summary>
        /// <param name="builderType">Type that will manage the build process</param>
        public BuildedWithAttribute(Type builderType)
        {
            BuilderType = builderType;
        }
    }
}