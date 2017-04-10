﻿using System;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Element
{
    public abstract class BaseElement : IElement, ICloneable
    {
        protected Type loadedType;
        private IElement next;
        private IElement previous;

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
                throw new NotSupportedException("This element doesn't support setting the load action.");
            }
        }

        public virtual Type LoadedType
        {
            get { return loadedType; }
            set { loadedType = value; }
        }

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
                throw new NotSupportedException("This element doesn't support setting the Store action.");
            }
        }

        public virtual bool IsIndexable
        {
            get { return LoadedType.IsArray; }
        }

        public IElement Next { get { return next; } }

        public IElement Previous { get { return previous; } }


        public IElement EnterArray(IElement index)
        {
            var arrayElement = (BaseElement)this[index];
            arrayElement.previous = this;
            next = arrayElement;
            return next;
        }

        public IElement this[IElement newIndex]
        {
            get
            {
                if (!IsIndexable)
                    throw new NotSupportedException("This element does not support indexing.");

                var newElement = (BaseElement)Clone();

                if (Index != null)
                {
                    newElement.Index = (GenericElement)((g, _) =>
                    {
                        Index.Load(g, TypeOfContent.Value);
                        g.LoadElementAddress(LoadedType);
                        newIndex.Load(g, TypeOfContent.Value);
                    });
                }
                else
                {
                    newElement.Index = newIndex;
                }

                return newElement;
            }
        }

        public IElement Lenght
        {
            get
            {
                if (previous == null)
                    throw new InvalidOperationException("This element is not contained inside an array.");

                GenericElement lenghtElement = new GenericElement(typeof(uint));

                lenghtElement.Load = (g, value) =>
                {
                    previous.Load(g, TypeOfContent.Value);
                    g.LoadLength(previous.LoadedType);
                };
                return lenghtElement;
            }
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
