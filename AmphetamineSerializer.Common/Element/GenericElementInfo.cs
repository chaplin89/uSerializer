using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Abstract  access an element.
    /// </summary>
    public class GenericElementInfo : IElementInfo
    {
        /// <summary>
        /// Initialize the object with action.
        /// </summary>
        /// <param name="loadAction"></param>
        /// <param name="storeAction"></param>
        public GenericElementInfo(Action<Emit, TypeOfContent> loadAction, Action<Emit, IElementInfo, TypeOfContent> storeAction)
        {
            Load = loadAction;
            Store = storeAction;
        }

        /// <summary>
        /// Initialize the object with null action.
        /// </summary>
        public GenericElementInfo()
        {

        }

        /// <summary>
        /// Action for emitting the load instruction(s).
        /// </summary>
        public Action<Emit, TypeOfContent> Load { get; set; }

        /// <summary>
        /// Action for emitting the store instructions.
        /// </remarks>
        public Action<Emit, IElementInfo, TypeOfContent> Store { get; set; }
    }
}
