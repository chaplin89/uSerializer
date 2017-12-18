using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer.Chain
{
    /// <summary>
    /// Response to a build request.
    /// </summary>
    public class SerializationBuildResponse : IResponse
    {
        /// <summary>
        /// Response to the request.
        /// </summary>
        public BuildedFunction Function { get; set; }
        
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
