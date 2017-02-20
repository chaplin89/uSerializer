using System;
using AmphetamineSerializer.Interfaces;
using System.Linq;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common
{
    public class SerializationBuildRequest : IRequest
    {
        private Type[] inputTypes;
        private Type outputType;
        private Type delegateType;

        public object AdditionalContext { get; set; }
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

        public Type[] InputTypes { get { return inputTypes; } }
        public Type OutputType { get { return outputType; } }
        public Type RootType { get { return inputTypes != null ? inputTypes.FirstOrDefault() : null; } }

        public ElementDescriptor Element { get; set; }
        public SigilFunctionProvider Provider { get; set; }
        public Emit G { get; set; }
    }
}
