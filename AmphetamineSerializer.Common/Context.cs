using AmphetamineSerializer.Interfaces;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Context of the assembly building process.
    /// </summary>
    public class Context
    {
        private Emit g;

        /// <summary>
        /// Build a context.
        /// </summary>
        /// <param name="inputParameters"></param>
        /// <param name="additionalContext"></param>
        /// <param name="element"></param>
        /// <param name="provider"></param>
        /// <param name="g"></param>
        public Context(Type[] inputParameters,
                              object additionalContext,
                              IElement element,
                              SigilFunctionProvider provider,
                              Emit g)
        {
            LoopCtx = new Stack<LoopContext>();
            InputParameters = inputParameters;
            AdditionalContext = additionalContext;
            CurrentElement = element;
            Provider = provider;
            G = g;
        }

        /// <summary>
        /// 
        /// </summary>
        public IElement CurrentElement { get; set; }

        /// <summary>
        /// Manage the contexs of loops.
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
        /// IL Generator for the current method.
        /// </summary>
        public Emit G
        {
            get { return g; }
            set
            {
                g = value;
                VariablePool = new VariablePool(g);
            }
        }

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
        /// </summary>
        /// <remarks>
        /// This means:
        ///     1) true if the input is passed ByRef
        ///     2) false vice-versa
        /// </remarks>
        public bool IsDeserializing
        {
            get
            {
                Debug.Assert(InputParameters != null && InputParameters.Length > 0);
                return InputParameters.First().IsByRef;
            }
        }

        /// <summary>
        /// Local variable pool for the current function.
        /// </summary>
        public VariablePool VariablePool { get; private set; }
    }
}