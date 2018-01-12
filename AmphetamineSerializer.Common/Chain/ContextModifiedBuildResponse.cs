using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// Response to a request that was handled by modifing the context, hence
    /// this does not contains any method.
    /// </summary>
    public class ContextModifiedBuildResponse : IResponse
    {
        /// <summary>
        /// Name of the module that processed the request.
        /// </summary>
        public string ProcessedBy { get; set; }
    }
}
