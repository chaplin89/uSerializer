using AmphetamineSerializer.Interfaces;
using System;

namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// Response for a delegate build request.
    /// </summary>
    public class DelegateBuildResponse : IResponse
    {
        /// <summary>
        /// Name of the module that processed the request.
        /// </summary>
        public string ProcessedBy { get; set; }

        /// <summary>
        /// Builded delegate.
        /// </summary>
        public Delegate Delegate { get; set; }
    }
}
