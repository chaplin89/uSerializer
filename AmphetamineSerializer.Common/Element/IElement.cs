using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Represent an abstract way for emitting instructions that load an elemenent in the stack
    /// or store in the element something taken from the stack.
    /// </summary>
    public interface IElement
    {
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
        /// If the field is an array, those element is loaded before accessing the element 
        /// o.
        /// </summary>
        IElement Index { get; set; }

        /// <summary>
        /// This is the type accessed when Index is null.
        /// </summary>
        Type RootType { get; set; }

        /// <summary>
        /// Type of the element that can be accessed using the current Index.
        /// </summary>
        /// <remarks>
        /// The element can be a trivial type (link an int or a float) or can be a very complex one 
        /// (like nested jagged arrays or matrixes). This field keep track of what is the type loaded with 
        /// the current Index.
        /// If this is a scalar type or Index is null, this is simply the type of the element.
        /// </remarks>
        Type ElementType { get; set; }
    }
}