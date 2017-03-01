using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Element
{
    public abstract class BaseElement : IElement
    {
        private IElement indexElement;

        public abstract Type ElementType { get; set; }

        public abstract IElement Index { get; set; }

        protected abstract Action<Emit, IElement, TypeOfContent> InternalStore(IElement index);

        protected abstract Action<Emit, TypeOfContent> InternalLoad(IElement index);

        public virtual Action<Emit, TypeOfContent> Load
        {
            get
            {
                return InternalLoad(Index);
            }
            set
            {
                throw new NotSupportedException("Can't set Load action.");
            }
        }

        public abstract Type RootType { get; set; }

        public virtual Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                return InternalStore(Index);
            }
            set
            {
                throw new NotSupportedException("This element doesn't support Store action.");
            }
        }

        public Action<Emit, TypeOfContent> LoadLenght()
        {
            return (g, value) =>
            {
                var load = InternalLoad(indexElement);
                load(g, TypeOfContent.Value);
                g.LoadLength(ElementType);
            };
        }

        public void EnterArray(IElement index)
        {
            if (Index == null)
            {
                Index = index;
            }
            else
            {
                indexElement = Index;

                Index = (GenericElement)((g, _) =>
                {
                    indexElement.Load(g, TypeOfContent.Value);
                    g.LoadElementAddress(ElementType);
                    index.Load(g, TypeOfContent.Value);
                });
            };
        }
    }
}
