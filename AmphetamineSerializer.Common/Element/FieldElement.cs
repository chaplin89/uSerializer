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
        /// Field information.
        /// </summary>
        public FieldInfo Field { get; set; }

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

        public override Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    Instance.Load(g, TypeOfContent.Value);
                    base.Load(g, content);
                };
            }
        }

        public override Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                return (g, value, content) =>
                {
                    Instance.Load(g, TypeOfContent.Value);
                    base.Store(g, value, content);
                };
            }
        }

        protected override void InternalStore(Emit g, TypeOfContent content)
        {
            g.StoreField(Field);
        }

        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            if (content == TypeOfContent.Value)
                g.LoadField(Field);
            else
                g.LoadFieldAddress(Field);
        }

        public override Type LoadedType
        {
            get { return Field?.FieldType; }
            set { throw new InvalidOperationException("RootType for FieldElement type is fixed."); }
        }
    }
}
