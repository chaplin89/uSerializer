using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// 
    /// </summary>
    public class SerializationBuildResponse : IResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public BuildedFunction Method { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public TypeOfRequest ResponseType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProcessedBy { get; set; }
    }
}
