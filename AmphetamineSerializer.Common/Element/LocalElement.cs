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
        private Type elementType;

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
                    if (LocalVariable.LocalType.IsArray && Index != null)
                    {
                        g.LoadLocal(LocalVariable);
                        Index.Load(g, TypeOfContent.Value);

                        if (content == TypeOfContent.Value)
                            g.LoadElement(ElementType);
                        else
                            g.LoadElementAddress(ElementType);
                    }
                    else
                    {
                        if (content == TypeOfContent.Value)
                            g.LoadLocal(LocalVariable);
                        else
                            g.LoadLocalAddress(LocalVariable);
                    }
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
                    if (LocalVariable.LocalType.IsArray && Index != null)
                    {
                        g.LoadLocal(LocalVariable);
                        Index.Load(g, TypeOfContent.Value);
                    }

                    value.Load(g, content);

                    if (LocalVariable.LocalType.IsArray && Index != null)
                        g.StoreElement(ElementType);
                    else
                        g.StoreLocal(LocalVariable);
                };
            }
        }

        /// <summary>
        /// <see cref="IElement.Index"/>
        /// </summary>
        public IElement Index { get; set; }

        /// <summary>
        /// <see cref="IElement.ElementType"/>
        /// </summary>
        public Type ElementType
        {
            get
            {
                if (elementType == null && LocalVariable != null)
                    elementType = LocalVariable.LocalType;
                return elementType;
            }
            set
            {
                elementType = value;
            }
        }
    }
}
