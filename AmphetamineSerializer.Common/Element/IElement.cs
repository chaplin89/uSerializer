using Sigil.NonGeneric;
using System;
using System.Collections.Generic;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Represent an abstract way for emitting instructions that load an elemenent in the stack
    /// or store in the element something taken from the stack.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// An IElement can be a very complex composition of nested arrays.
        /// While enumerating all the nodes in its graph, this contain all the nodes
        /// back to its root. 
        /// </summary>
        IElement Next { get; }

        IElement Previous { get; }

        bool IsIndexable { get; }

        IElement Index { get; }


        IElement Lenght { get; }

        /// <summary>
        /// Action for emitting instructions for loading an element into the stack.
        /// </summary>
        Action<Emit, TypeOfContent> Load { get; }

        /// <summary>
        /// Action for emitting instructions for storing the element from the stack.
        /// </summary>
        /// <remarks>
        /// The action has the following signature:
        /// void Store(Emit, IElement, TypeOfContent):
        /// 1. Emit, is the object for emitting instructions
        /// 2. IElement, represent the element that will be stored inside this element.
        /// 3. TypeOfContent, tell if the value to store should be loaded by value or by address.
        /// </remarks>
        Action<Emit, IElement, TypeOfContent> Store { get; }

        IElement EnterArray(IElement index);

        /// <summary>
        /// Type of the object loaded when called Load.
        /// </summary>
        Type LoadedType { get; set; }
    }
}