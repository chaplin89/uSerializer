using System;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// 
    /// </summary>
    public class ArgumentElement : IElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argumentIndex"></param>
        public ArgumentElement(ushort argumentIndex)
        {
            ArgumentIndex = argumentIndex;
        }

        /// <summary>
        /// Index of the argument
        /// </summary>
        public ushort ArgumentIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (content == TypeOfContent.Value)
                        g.LoadArgument(ArgumentIndex);
                    else
                        g.LoadArgumentAddress(ArgumentIndex);
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                return (g, element, content) =>
                {
                    element.Load(g, content);
                    g.StoreArgument(ArgumentIndex);
                };
            }
        }
    }
}
