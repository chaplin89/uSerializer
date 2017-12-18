using AmphetamineSerializer.Model;
using AmphetamineSerializer.Model.Attributes;
using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Interfaces
{
    /// <summary>
    /// Represent an abstract way for emitting instructions that load an elemenent in the stack
    /// or store in the element something taken from the stack.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Next IElement in the chain.
        /// </summary>
        /// <remarks>
        /// An IElement can be obtained applying multiple indexes to another IElement.
        /// Every time an index is applied, the consequence is that another node is added
        /// to the chain in order to keep track of the derivation of the element back to
        /// its root.
        /// </remarks>
        IElement Next { get; }

        /// <summary>
        /// Previous IElement in the chain.
        /// </summary>
        /// <seealso cref="IElement.Next"/>
        IElement Previous { get; }

        /// <summary>
        /// Specify if the current element can be indexed.
        /// </summary>
        bool IsIndexable { get; }

        /// <summary>
        /// Current index used while loading this element.
        /// </summary>
        IElement Index { get; }

        /// <summary>
        /// If this element is an array, this IElement allow to load its lenght.
        /// </summary>
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

        /// <summary>
        /// Make another node in the chain applying an index to this IElement.
        /// </summary>
        /// <param name="index">Index to apply</param>
        /// <returns>The new IElement</returns>
        IElement EnterArray(IElement index);

        /// <summary>
        /// Type of the object loaded when called Load.
        /// </summary>
        Type LoadedType { get; set; }

        /// <summary>
        /// Meta-information about this element.
        /// </summary>
        ASIndexAttribute Attribute { get; set; }
    }
}