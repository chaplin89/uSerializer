using System;
using System.Collections.Generic;
using System.Linq;
using Sigil.NonGeneric;
using System.Diagnostics;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Context of the assembly building process.
    /// </summary>
    public class FoundryContext
    {
        private ILAbstraction manipulator;

        private FoundryContext()
        {
            LoopCtx = new Stack<LoopContext>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegateType"></param>
        /// <param name="additionalContext"></param>
        /// <returns></returns>
        public static FoundryContext MakeContext(Type delegateType, 
                                                 object additionalContext,
                                                 FieldElement element, 
                                                 SigilFunctionProvider provider,
                                                 Emit g)
        {
            return new FoundryContext()
            {
                InputParameters = delegateType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray(),
                AdditionalContext = additionalContext,
                Element = element,
                Provider = provider,
                G = g
            };
        }

        public FieldElement Element;

        /// <summary>
        /// 
        /// </summary>
        public Stack<LoopContext> LoopCtx { get; set; }

        /// <summary>
        /// Type upon wich build the deserialization logic.
        /// </summary>
        public Type ObjectType
        {
            get
            {
                if (InputParameters != null)
                    return InputParameters.FirstOrDefault();
                return null;
            }
        }

        /// <summary>
        /// Provide basic abstraction from IL.
        /// </summary>
        public ILAbstraction Manipulator
        {
            get
            {
                if (manipulator == null)
                    manipulator = new ILAbstraction(G);
                return manipulator;
            }
        }

        /// <summary>
        /// Additional context used in custom builders (if not null).
        /// </summary>
        /// <remarks>
        /// The meaning is dependent on the builder.
        /// Common case when you need to use this field is when you're building 
        /// your serialization logic starting from generic meta-information retrieved 
        /// from the header of a file (and not directly from an object graph).
        /// </remarks>
        public object AdditionalContext { get; set; }

        /// <summary>
        /// Provide an easy way to build a function from scratch.
        /// </summary>
        public SigilFunctionProvider Provider { get; set; }

        /// <summary>
        /// Generator.
        /// </summary>
        public Emit G { get;set;}

        /// <summary>
        /// Types in input to the method that will be forwarded to the
        /// serialization handlers.
        /// </summary>
        public Type[] InputParameters { get; set; }

        /// <summary>
        /// Chain-of-responsibility that manage building request.
        /// </summary>
        public IChainManager Chain { get; set; }

        /// <summary>
        /// Indicate wether the builder is managing the life-cycle of the elements.
        /// For the moment, this means:
        /// 1) true if the input is passed ByRef
        /// 2) false vice-versa
        /// </summary>
        /// <remarks>
        /// This is the only thing that differentiate a Serialization from a Deserialization.
        /// The inner part of the builder, in fact, does not know anything about Serialization or 
        /// Deserialization because it's only a way to iterate over an object graph and call some methods.
        /// </remarks>
        public bool ManageLifeCycle
        {
            get
            {
                Debug.Assert(InputParameters != null && InputParameters.Length > 0);
                return InputParameters.First().IsByRef;
            }
        }

        /// <summary>
        /// CurrentUnderlyingType 
        /// </summary>
        public Type NormalizedType {
            get
            {
                Type normalizedType = Element.UnderlyingType;

                if (Element.UnderlyingType.IsEnum)
                    normalizedType = normalizedType.GetEnumUnderlyingType();
                if (ManageLifeCycle)
                    normalizedType = normalizedType.MakeByRefType();

                return normalizedType;
            }
        }
    }
}
