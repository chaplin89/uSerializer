using Sigil.NonGeneric;
using System;
using System.Reflection;
using AmphetamineSerializer.Common.Attributes;
using System.Collections.Generic;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Manage a class field.
    /// </summary>
    public class FieldElement : IElement
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
        public IElement Index { get; set; }

        /// <summary>
        /// Field information.
        /// </summary>
        public FieldInfo Field { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Type ElementType
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

        /// <summary>
        /// Emit instructions to load a field in the stack.
        /// TODO: Manage matrix.
        /// </summary>
        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    Instance.Load(g, TypeOfContent.Value);

                    if (Field.FieldType.IsArray && Index != null)
                    {
                        g.LoadField(Field);
                        Index.Load(g, TypeOfContent.Value);

                        if (content == TypeOfContent.Value)
                            g.LoadElement(Field.FieldType.GetElementType());
                        else
                            g.LoadElementAddress(Field.FieldType.GetElementType());
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
        }

        /// <summary>
        /// Emit instructions for storing something taken from the stack in a field of a class.
        /// </summary>
        public Action<Emit, IElement, TypeOfContent> Store
        {
            get
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
                        g.StoreElement(Field.FieldType.GetElementType());
                    else
                        g.StoreField(Field);
                };
            }
        }
    }
}
