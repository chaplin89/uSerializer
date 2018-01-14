using AmphetamineSerializer.Interfaces;
using System.Reflection;

namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// Response that contains a method that belong to a finalized type.
    /// </summary>
    public class TypeFinalizedBuildResponse : IResponse
    {
        /// <summary>
        /// Builded method.
        /// </summary>
        public MethodInfo Method { get; set; }
        
        /// <summary>
        /// If the method is not static, this is the instance.
        /// </summary>
        public IElement Instance { get; set; }

        /// <summary>
        /// Name of the module that processed the request.
        /// </summary>
        public string ProcessedBy { get; set; }
    }
}
