using Sigil.NonGeneric;
using System;
using System.Reflection;
using AmphetamineSerializer.Common.Attributes;
using AmphetamineSerializer.Common.Element;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Manage a class field.
    /// </summary>
    public class FieldElement : BaseElement
    {
        private Type elementType;

        /// <summary>
        /// Build this object and initialize instance and field.
        /// </summary>
        /// <param name="instance">Instance of the object that contain the field</param>
        /// <param name="field">The field</param>
        public FieldElement(IElement instance, FieldInfo field)
        {
            Instance = instance;
            Field = field;
        }

        /// <summary>
        /// Build this object with null instance/field.
        /// </summary>
        public FieldElement()
        {
        }

        /// <summary>
        /// Object instance.
        /// </summary>
        public IElement Instance { get; set; }

        /// <summary>
        /// If the field is an array, 
        /// this is the index(es) used in load/store.
        /// </summary>
        public override IElement Index { get; set; }

        /// <summary>
        /// Field information.
        /// </summary>
        public FieldInfo Field { get; set; }

        /// <summary>
        /// Type of the element.
        /// <seealso cref="IElement.ElementType"/>
        /// </summary>
        public override Type ElementType
        {
            get
            {
                if (elementType == null)
                    elementType = Field.FieldType;
                return elementType;
            }
            set
            {
                elementType = value;
            }
        }

        /// <summary>
        /// Access the ASIndexAttribute of the field.
        /// </summary>
        public ASIndexAttribute Attribute
        {
            get
            {
                if (Field == null)
                    return null;
                return Field.GetCustomAttribute<ASIndexAttribute>();
            }
        }

        protected override Action<Emit, TypeOfContent> InternalLoad(IElement index)
        {
            return (g, content) =>
            {
                Instance.Load(g, TypeOfContent.Value);
            
                if (Field.FieldType.IsArray && index != null)
                {
                    g.LoadField(Field);
                    index.Load(g, TypeOfContent.Value);

                    if (content == TypeOfContent.Value)
                        g.LoadElement(ElementType);
                    else
                        g.LoadElementAddress(ElementType);
                }
                else
                {
                    if (content == TypeOfContent.Value)
                        g.LoadField(Field);
                    else
                        g.LoadFieldAddress(Field);
                }
            };
        }

        protected override Action<Emit, IElement, TypeOfContent> InternalStore(IElement index)
        {
            return (g, value, content) =>
            {
                Instance.Load(g, TypeOfContent.Value);

                if (Field.FieldType.IsArray)
                {
                    g.LoadField(Field);
                    Index.Load(g, TypeOfContent.Value);
                }

                value.Load(g, content);

                if (Field.FieldType.IsArray)
                    g.StoreElement(ElementType);
                else
                    g.StoreField(Field);
            };
        }

        public override Type RootType
        {
            get { return Field?.FieldType; }
            set { throw new InvalidOperationException("RootType for FieldElement type is fixed."); }
        }
    }
}
