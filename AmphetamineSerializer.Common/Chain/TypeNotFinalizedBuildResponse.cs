using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using Sigil.NonGeneric;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// Response that contains a method that belong to a type that 
    /// is not finalized.
    /// </summary>
    public class TypeNotFinalizedBuildResponse : IResponse
    {
        /// <summary>
        /// Name of the module that processed the request.
        /// </summary>
        public string ProcessedBy { get; set; }

        /// <summary>
        /// Status of the builded function.
        /// </summary>
        public BuildedFunctionStatus Status { get; set; }

        /// <summary>
        /// Emiter that can be use to manipulate the funcion (if the
        /// function is not finalized) or invoke the method (either 
        /// the funcion is finalized or not).
        /// </summary>
        public Emit Emiter { get; set; }

        /// <summary>
        /// Input types.
        /// </summary>
        public Type[] Input { get; set; }

        /// <summary>
        /// Output type.
        /// </summary>
        public Type Return { get; set; }
    }
}
