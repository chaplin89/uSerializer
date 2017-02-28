using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Abstract  access an element.
    /// </summary>
    public class GenericElementInfo : IElementInfo
    {
        #region Conversions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="load"></param>
        public static implicit operator GenericElementInfo(Action<Emit, TypeOfContent> load)
        {
            return new GenericElementInfo(load, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="store"></param>
        public static implicit operator GenericElementInfo(Action<Emit, IElementInfo, TypeOfContent> store)
        {
            return new GenericElementInfo(null, store);
        }
        #endregion

        /// <summary>
        /// Initialize the object with actions.
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
