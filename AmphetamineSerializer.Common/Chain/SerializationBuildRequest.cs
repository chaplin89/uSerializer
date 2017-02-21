using System;
using AmphetamineSerializer.Interfaces;
using System.Linq;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Chain
{
    public class SerializationBuildRequest : IRequest
    {
        private Type[] inputTypes;
        private Type outputType;
        private Type delegateType;

        /// <summary>
        /// 
        /// </summary>
        public object AdditionalContext { get; set; }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public Type[] InputTypes { get { return inputTypes; } }

        /// <summary>
        /// 
        /// </summary>
        public Type OutputType { get { return outputType; } }

        /// <summary>
        /// 
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

        public ElementDescriptor Element { get; set; }

        public SigilFunctionProvider Provider { get; set; }

        public Emit G { get; set; }

        TypeOfRequest RequestType { get; set; }
    }
}
