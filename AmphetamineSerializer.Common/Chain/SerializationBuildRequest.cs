using System;
using AmphetamineSerializer.Interfaces;
using System.Linq;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// Request to build a serializator.
    /// </summary>
    public class SerializationBuildRequest : IRequest
    {
        private Type[] inputTypes;
        private Type outputType;
        private Type delegateType;

        /// <summary>
        /// Additional context that will be passed to the builder.
        /// </summary>
        public object AdditionalContext { get; set; }

        /// <summary>
        /// Build a request based on a delegate type.
        /// </summary>
        public Type DelegateType {
            get { return delegateType; }
            set
            {
                var method = value.GetMethod("Invoke");
                inputTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
                outputType = method.ReturnType;
                delegateType = value;
            }
        }

        /// <summary>
        /// Input type for this request.
        /// </summary>
        public Type[] InputTypes { get { return inputTypes; } }

        /// <summary>
        /// Output type for this request.
        /// </summary>
        public Type OutputType { get { return outputType; } }

        /// <summary>
        /// Root type that is will be processed by this request.
        /// </summary>
        public Type RootType
        {
            get
            {
                if (inputTypes != null)
                    return inputTypes.FirstOrDefault();
                return null;
            }
        }

        /// <summary>
        /// If the request allow the process to modify the a context,
        /// this will contain information about the context.
        /// </summary>
        public ElementDescriptor Element { get; set; }

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
        /// Type of request.
        /// </summary>
        public TypeOfRequest RequestType { get; set; }
    }
}
