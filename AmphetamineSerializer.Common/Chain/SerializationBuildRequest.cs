using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Chain
{
    public class ElementBuildRequest : IRequest
    {
        /// <summary>
        /// Additional context that will be passed to the builder.
        /// </summary>
        public object AdditionalContext { get; set; }

        /// <summary>
        /// If the request allow the process to modify the a context,
        /// this will contain information about the context.
        /// </summary>
        public IElement Element { get; set; }

        /// <summary>
        /// If the request allow the process to modify the a context,
        /// this will contain the provider.
        /// </summary>
        public SigilFunctionProvider Provider { get; set; }

        /// <summary>
        /// If the request allow the process to modify the a context,
        /// this will contain the generator.
        /// </summary>
        public Emit G { get; set; }

        /// <summary>
        /// Build a request based on a delegate type.
        /// </summary>
        public Type[] InputTypes { get; set; }
    }

    /// <summary>
    /// Request to build a serializator.
    /// </summary>
    public class DelegateBuildRequest : IRequest
    {
        /// <summary>
        /// Additional context that will be passed to the builder.
        /// </summary>
        public object AdditionalContext { get; set; }

        /// <summary>
        /// Build a request based on a delegate type.
        /// </summary>
        public Type DelegateType { get; set; }
    }
}