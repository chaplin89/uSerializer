using System;
using AmphetamineSerializer.Model.Attributes;
using Sigil.NonGeneric;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;

namespace AmphetamineSerializer.Common.Element
{
    public abstract class BaseElement : IElement, ICloneable
    {
        protected Type loadedType;
        private IElement next;
        private IElement previous;

        /// <summary>
        /// Current index for accessing this element.
        /// </summary>
        public virtual IElement Index { get; set; }

        protected abstract void InternalStore(Emit g, TypeOfContent content);

        protected abstract void InternalLoad(Emit g, TypeOfContent content);

        public virtual void Load(Emit g, TypeOfContent content)
        {
            if (Index != null)
            {
                InternalLoad(g, TypeOfContent.Value);

                if (loadedType.IsByRef)
                    g.LoadIndirect(loadedType.GetElementType());

                Index.Load(g, TypeOfContent.Value);

                if (content == TypeOfContent.Value)
                    g.LoadElement(LoadedType);
                else
                    g.LoadElementAddress(LoadedType);
            }
            else
            {
                if (loadedType.IsByRef)
                {
                    InternalLoad(g, TypeOfContent.Value);
                    if (content == TypeOfContent.Value)
                        g.LoadIndirect(loadedType.GetElementType());
                }
                else
                {
                    InternalLoad(g, content);
                }
            }
        }

        public virtual Type LoadedType
        {
            get { return loadedType; }
            set { loadedType = value; }
        }

        public virtual void Store(Emit g, IElement value, TypeOfContent content)
        {
            if (Index != null || loadedType.IsByRef)
            {
                InternalLoad(g, TypeOfContent.Value);

                if (Index != null && loadedType.IsByRef)
                    g.LoadIndirect(loadedType.GetElementType());
            }

            Index?.Load(g, TypeOfContent.Value);
            value.Load(g, content);

            if (Index != null)
            {
                g.StoreElement(LoadedType);
            }
            else
            {
                if (loadedType.IsByRef)
                    g.StoreIndirect(loadedType.GetElementType());
                else
                    InternalStore(g, TypeOfContent.Value);
            }
        }

        public virtual bool IsIndexable
        {
            get
            {
                return LoadedType.IsArray;
            }
        }

        public IElement Next { get { return next; } }

        public IElement Previous { get { return previous; } }

        public IElement EnterArray(IElement index)
        {
            if (!IsIndexable)
                throw new NotSupportedException("This element does not support indexing.");

            var arrayElement = (BaseElement)this[index];
            arrayElement.previous = this;
            next = arrayElement;
            arrayElement.loadedType = loadedType.GetElementType();
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
                    newElement.Index = new GenericElement(((g, _) =>
                    {
                        Index.Load(g, TypeOfContent.Value);
                        g.LoadElement(LoadedType);
                        newIndex.Load(g, TypeOfContent.Value);
                    }), null);
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
                if (!IsIndexable)
                    throw new InvalidOperationException("This element is not contained inside an array.");

                GenericElement lenghtElement = new GenericElement(typeof(uint));

                lenghtElement.LoadAction = (g, value) =>
                {
                    Load(g, TypeOfContent.Value);
                    g.LoadLength(LoadedType.GetElementType());
                };
                return lenghtElement;
            }
        }

        public abstract ASIndexAttribute Attribute { get; set; }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
