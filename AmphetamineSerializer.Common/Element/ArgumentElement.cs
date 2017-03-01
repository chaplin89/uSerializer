using System;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage an argument.
    /// </summary>
    public class ArgumentElement : IElement
    {
        Type rootType;
        Type elementType;

        /// <summary>
        /// Build a wrapper aroung an argument index.
        /// </summary>
        /// <param name="argumentIndex">Index of the argument.</param>
        public ArgumentElement(ushort argumentIndex)
        {
            ArgumentIndex = argumentIndex;
        }

        /// <summary>
        /// Index of the argument
        /// </summary>
        public ushort ArgumentIndex { get; set; }

        /// <summary>
        /// Type of the argument element.
        /// </summary>
        /// <remarks>Should be set manually because there is no way to deduce it.</remarks>
        public Type ElementType
        {
            get
            {
                return elementType;
            }
            set
            {
                if (rootType == null)
                    rootType = value;
                elementType = value;
            }
        }

        /// <summary>
        /// <see cref="IElement.Index"/>
        /// </summary>
        public IElement Index { get; set; }

        /// <summary>
        /// <see cref="IElement.Load"/>
        /// </summary>
        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (rootType.IsArray && Index != null)
                    {
                        g.LoadArgument(ArgumentIndex);
                        Index.Load(g, TypeOfContent.Value);

                        if (content == TypeOfContent.Value)
                            g.LoadElement(ElementType);
                        else
                            g.LoadElementAddress(ElementType);
                    }
                    else
                    {
                        if (content == TypeOfContent.Value)
                            g.LoadArgument(ArgumentIndex);
                        else
                            g.LoadArgumentAddress(ArgumentIndex);
                    }
                };
            }
        }

        /// <summary>
        /// <see cref="IElement.Store"/>
        /// </summary>
        public Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                return (g, value, content) =>
                {
                    if (rootType.IsArray && Index != null)
                    {
                        g.LoadArgument(ArgumentIndex);
                        Index.Load(g, TypeOfContent.Value);
                    }

                    value.Load(g, content);

                    if (rootType.IsArray && Index != null)
                        g.StoreElement(ElementType);
                    else
                        g.StoreArgument(ArgumentIndex);
                };
            }
        }
    }
}
