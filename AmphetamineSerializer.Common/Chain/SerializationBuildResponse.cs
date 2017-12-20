using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using Sigil.NonGeneric;
using System;
using System.Reflection;

namespace AmphetamineSerializer.Chain
{
    public class DelegateBuildResponse : IResponse
    {
        public Delegate Delegate { get; set; }
        public string ProcessedBy { get; set; }
    }

    /// <summary>
    /// Response to a build request.
    /// </summary>
    public class ElementBuildResponse : IResponse
    {
        public BuildedFunctionStatus Status { get; set; }

        public Emit Emiter { get; set; }

        public Type[] Input { get; set; }

        public MethodInfo Method { get; set; }

        public Type Return { get; set; }

        /// <summary>
        /// If the method is not static, this is the instance.
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// Name of the module that processed the request.
        /// </summary>
        /// <remarks>Used for debug purpouse only.</remarks>
        public string ProcessedBy { get; set; }
    }
}
