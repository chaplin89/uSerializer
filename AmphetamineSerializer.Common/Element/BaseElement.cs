using System;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Element
{
    public abstract class BaseElement : IElement
    {
        private BaseElement next;
        private BaseElement previous;

        public virtual IElement Index { get; set; }

        protected abstract void InternalStore(Emit g, TypeOfContent content);

        protected abstract void InternalLoad(Emit g, TypeOfContent content);

        public virtual Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (Index != null)
                    {
                        InternalLoad(g, TypeOfContent.Value);
                        Index.Load(g, TypeOfContent.Value);

                        if (content == TypeOfContent.Value)
                            g.LoadElement(LoadedType);
                        else
                            g.LoadElementAddress(LoadedType);
                    }
                    else
                    {
                        InternalLoad(g, content);
                    }
                };
            }
            set
            {
                throw new NotSupportedException("Can't set Load action.");
            }
        }

        public abstract Type LoadedType { get; set; }

        public virtual Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                return (g, value, content) =>
                {
                    if (Index != null)
                    {
                        InternalLoad(g, TypeOfContent.Value);
                        Index.Load(g, TypeOfContent.Value);
                    }

                    value.Load(g, content);

                    if (Index != null)
                        g.StoreElement(LoadedType);
                    else
                        InternalStore(g, TypeOfContent.Value);
                };
            }
            set
            {
                throw new NotSupportedException("This element doesn't support Store action.");
            }
        }
        
        public virtual bool IsIndexable
        {
            get { return LoadedType.IsArray; }
        }

        public IElement Next { get { return next; } }

        public IElement Previous { get { return previous; } }
        
        public void EnterArray(IElement index)
        {
            if (Index == null)
            {
                Index = index;
            }
            else
            {
                var indexElement = Index;

                // Index = (GenericElement)((g, _) =>
                // {
                //     indexElement.Load(g, TypeOfContent.Value);
                //     g.LoadElementAddress(ElementType);
                //     Index.Load(g, TypeOfContent.Value);
                // });
            };
        }

        public Action<Emit, TypeOfContent> LoadArrayLenght()
        {
            return (g, value) =>
            {
                previous.InternalLoad(g, TypeOfContent.Value);
                g.LoadLength(previous.LoadedType);
            };
        }
    }
}
