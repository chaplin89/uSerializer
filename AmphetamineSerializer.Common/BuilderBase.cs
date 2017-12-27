using AmphetamineSerializer.Chain;
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
        protected Context ctx;

        /// <summary>
        /// Internally cached method.
        /// </summary>
        protected ElementBuildResponse method;

        /// <summary>
        /// Construct a builder with a context.
        /// </summary>
        /// <param name="ctx">Context</param>
        public BuilderBase(Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Return a cached function or build a new one.
        /// </summary>
        /// <returns>The function</returns>
        public IResponse Make()
        {
            if (method == null)
                method = InternalMake();

            return method;
        }

        /// <summary>
        /// Build a function.
        /// </summary>
        /// <returns>The function</returns>
        protected abstract ElementBuildResponse InternalMake();
    }
}
