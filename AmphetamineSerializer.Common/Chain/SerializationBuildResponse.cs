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
        public BuildedFunction Response { get; set; }
        
        /// <summary>
        /// Tell how the response has been made.
        /// </summary>
        public TypeOfRequest ResponseType { get; set; }

        /// <summary>
        /// If the method is not static, this is the instance.
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// Name of the module that processed the request.
        /// </summary>
        public string ProcessedBy { get; set; }
    }
}
