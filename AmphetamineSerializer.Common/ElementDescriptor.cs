using AmphetamineSerializer.Common.Attributes;
using Sigil;
using System;
using System.Reflection;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common
{
    public enum ElementType
    {
        Field,
        Local,
        Custom
    }

    /// <summary>
    /// Describe an element that is being processed.
    /// </summary>
    public struct ElementDescriptor
    {
        FieldElement fieldElement;
        Local localVariable;
        GenericElement customElement;

        /// <summary>
        /// Map a field to the instance of the type that contain the field. 
        /// </summary>
        public FieldElement FieldElement
        {
            get
            {
                return fieldElement;
            }
            set
            {
                fieldElement = value;
                localVariable = null;
                customElement = null;
                ElementType = ElementType.Field;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Local LocalVariable
        {
            get
            {
                return localVariable;
            }
            set
            {
                localVariable = value;
                customElement = null;
                fieldElement = null;
                ElementType = ElementType.Local;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public GenericElement CustomElement
        {
            get
            {
                return customElement;
            }
            set
            {
                customElement = value;
                localVariable = null;
                fieldElement = null;
                ElementType = ElementType.Custom;
            }
        }

        /// <summary>
        /// When building the deserializing logic for a type, 
        /// this field contains the type for the currently processed
        /// field.
        /// </summary>
        public Type ItemType { get; set; }

        /// <summary>
        /// If it's an array or an enum, this field contains the type
        /// contained inside.
        /// </summary>
        public Type UnderlyingType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ElementType ElementType { get; set; }

        /// <summary>
        /// Return the attibute for current field.
        /// </summary>
        public ASIndexAttribute CurrentAttribute
        {
            get
            {
                if (ElementType != ElementType.Field)
                    throw new InvalidOperationException("This element is not a field.");
                if (FieldElement == null)
                    throw new InvalidOperationException("This element is a field but there are no info.");

                if (FieldElement.Field != null)
                    return FieldElement.Field.GetCustomAttribute<ASIndexAttribute>();
                return null;
            }
        }

        public LoopContext LoopCtx { get; set; }
    }
}