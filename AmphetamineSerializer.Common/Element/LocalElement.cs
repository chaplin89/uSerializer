using System;
using Sigil.NonGeneric;
using Sigil;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage a local variable.
    /// </summary>
    public class LocalElement : IElement
    {
        /// <summary>
        /// Build a LocalElement wrapper around a Local variable.
        /// </summary>
        /// <param name="local">The local variable</param>
        public static implicit operator LocalElement(Local local)
        {
            return new LocalElement(local);
        }

        /// <summary>
        /// Convert this object to its contained variable.
        /// </summary>
        /// <param name="local">The local variable</param>
        public static implicit operator Local(LocalElement local)
        {
            return local.LocalVariable;
        }

        /// <summary>
        /// Build this object with a local variable.
        /// </summary>
        /// <param name="local">The local variable</param>
        public LocalElement(Local local)
        {
            LocalVariable = local;
        }
        
        /// <summary>
        /// The local variable
        /// </summary>
        public Local LocalVariable { get; set; }

        /// <summary>
        /// Emit instructions for loading the local variable in the stack.
        /// </summary>
        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (content == TypeOfContent.Value)
                        g.LoadLocal(LocalVariable);
                    else
                        g.LoadLocalAddress(LocalVariable);
                };
            }
        }

        /// <summary>
        /// Emit instructions for storing something, taken from the stack, in the local variable.
        /// </summary>
        public Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                return (g, value, content) =>
                {
                    value.Load(g, content);
                    g.StoreLocal(LocalVariable);
                };
            }
        }
    }
}
