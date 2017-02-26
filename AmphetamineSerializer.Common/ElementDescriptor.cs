using AmphetamineSerializer.Common.Attributes;
using Sigil;
using System;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    public enum ElementType
    {
        Field,
        Local,
        Custom
    }

    /// <summary>
    /// Represent the tuple object instance, field.
    /// </summary>
    public class FieldElementInfo : GenericElementInfo
    {
        public FieldElementInfo()
        {
            base.LoadAction = this.Load;

        }
        /// <summary>
        /// Object instance.
        /// </summary>
        public Local Instance { get; set; }

        /// <summary>
        /// Field information.
        /// </summary>
        public FieldInfo Field { get; set; }

        void Load(FoundryContext ctx, TypeOfContent content)
        {

        }

        void Store(FoundryContext ctx, IElementInfo value)
        {

        }
    }

    /// <summary>
    /// Abstract  access an element.
    /// </summary>
    public class GenericElementInfo : IElementInfo
    {
        public GenericElementInfo(Action<FoundryContext, TypeOfContent> loadAction, Action<FoundryContext, IElementInfo> storeAction )
        {
            LoadAction = loadAction;
            StoreAction = storeAction;
        }

        protected GenericElementInfo()
        {
        }


        /// <summary>
        /// Action for emitting instruction 
        /// for loading an element into the stack.
        /// </summary>
        public Action<FoundryContext, TypeOfContent> LoadAction { get; protected set; }

        /// <summary>
        /// Action for emitting instructions
        /// for storing the element from the stack.
        /// </summary>        
        /// <remarks>
        /// The action has the following signature:
        /// void Store(FoundryContext, IElementInfo).
        /// The IElementInfo represent the element that 
        /// will be stored inside this one.
        /// </remarks>
        public Action<FoundryContext, IElementInfo> StoreAction { get; protected set; }
    }

    /// <summary>
    /// Describe an element that is being processed.
    /// </summary>
    public struct ElementDescriptor
    {
        FieldElementInfo fieldElement;
        Local localVariable;
        GenericElementInfo customElement;

        /// <summary>
        /// Map a field to the instance of the type that contain the field. 
        /// </summary>
        public FieldElementInfo FieldElement
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
        public GenericElementInfo CustomElement
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