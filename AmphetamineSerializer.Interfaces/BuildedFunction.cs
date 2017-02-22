using Sigil.NonGeneric;
using System.Reflection;
using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Represent the status about a function.
    /// </summary>
    public enum BuildedFunctionStatus
    {
        /// <summary>
        /// Initial status.
        /// </summary>
        NoMethodsAvailable,

        /// <summary>
        /// Emit was created and it is still editable so no MethodInfo is available.
        /// </summary>
        FunctionNotFinalized,

        /// <summary>
        /// CreateMethod was was called on the Emit so the Emit is not editable.
        /// MethodInfo is available, but all the calls to this method should be done 
        /// with the Emit before CreateType is called.
        /// </summary>
        FunctionFinalizedTypeNotFinalized,

        /// <summary>
        /// The type is created and it is not possible to use the Emit.
        /// All the calls should be done with the MethodInfo object.
        /// </summary>
        TypeFinalized
    }

    /// <summary>
    /// Need to be here because it's needed by the interface IBuilder.
    /// Need refactoring.
    /// </summary>
    public class BuildedFunction
    {
        public Emit Emiter { get; set; }
        public Type[] Input { get; set; }
        public MethodInfo Method { get; set; }
        public Type Return { get; set; }
        public BuildedFunctionStatus Status { get; set; }
    }
}
