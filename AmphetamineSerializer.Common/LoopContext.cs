using Sigil;
using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Manage a context for a loop
    /// </summary>
    public class LoopContext : IDisposable
    {
        /// <summary>
        /// This is the index.
        /// </summary>
        public Local Index { get; set; }

        /// <summary>
        /// Label that point to the end of the loop,
        /// where the out of bound condition is checked.
        /// </summary>
        public Label CheckOutOfBound { get; set; }

        /// <summary>
        /// Label that point at the body of the loop.
        /// </summary>
        public Label Body { get; set; }

        /// <summary>
        /// Size of the array.
        /// </summary>
        public Local Size { get; set; }

        /// <summary>
        /// If it's not null, CurrentItemFieldInfo is assumed to be
        /// an array, and the loopManager won't try to set the
        /// </summary>
        public int? StoreAtPosition { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
