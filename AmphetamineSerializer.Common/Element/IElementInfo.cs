using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Common
{
    public interface IElementInfo
    {
        /// <summary>
        /// Action for emitting instruction 
        /// for loading an element into the stack.
        /// </summary>
        Action<Emit, TypeOfContent> Load { get; }

        /// <summary>
        /// Action for emitting instructions
        /// for storing the element from the stack.
        /// </summary>        
        /// <remarks>
        /// The action has the following signature:
        /// void Store(FoundryContext, IElementInfo).
        /// The IElementInfo represent the element that 
        /// will be stored inside this one.
        /// </remarks>
        Action<Emit, IElementInfo, TypeOfContent> Store { get; }
    }
}