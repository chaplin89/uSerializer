using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Base class for builder types.
    /// </summary>
    public abstract class BuilderBase : IBuilder
    {
        /// <summary>
        /// Context for the current building process.
        /// </summary>
        protected FoundryContext ctx;

        /// <summary>
        /// Internally cached method.
        /// </summary>
        protected BuildedFunction method;

        /// <summary>
        /// Construct a builder with a context.
        /// </summary>
        /// <param name="ctx">Context</param>
        public BuilderBase(FoundryContext ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Build a function based on its context.
        /// </summary>
        /// <returns></returns>
        public abstract BuildedFunction Make();
    }
}
