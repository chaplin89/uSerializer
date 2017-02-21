using System;
using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer.Common
{
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

        public BuilderBase(FoundryContext ctx)
        {
            this.ctx = ctx;
        }

        public abstract BuildedFunction Make();
    }
}
