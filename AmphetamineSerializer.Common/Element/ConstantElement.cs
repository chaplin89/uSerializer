using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using AmphetamineSerializer.Model.Attributes;
using Sigil.NonGeneric;
using System;
using System.Reflection;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage the load of a constant value in the stack.
    /// </summary>
    /// <typeparam name="ConstantType">Type of constant value.</typeparam>
    public class ConstantElement<ConstantType> : BaseElement
    {
        MethodInfo method;

        /// <summary>
        /// Build a ConstantElement wrapper arount a constant.
        /// </summary>
        /// <param name="constant">The constant.</param>
        public static implicit operator ConstantElement<ConstantType>(ConstantType constant)
        {
            return new ConstantElement<ConstantType>(constant);
        }

        /// <summary>
        /// Return the constant contained in this object.
        /// </summary>
        /// <param name="constant">The constant.</param>
        public static implicit operator ConstantType(ConstantElement<ConstantType> constant)
        {
            return constant.Constant;
        }

        /// <summary>
        /// Build a ConstantElement object initializing the constant.
        /// </summary>
        /// <param name="constant">Initial constant.</param>
        /// <remarks>
        /// There should exist the method LoadConstant(ConstantType) inside the Emit class.
        /// </remarks>
        /// <exception cref="NotSupportedException">If the method for emitting the constant was not found.</exception>
        public ConstantElement(ConstantType constant)
        {
            Constant = constant;
            method = typeof(Emit).GetMethod("LoadConstant", new Type[] { typeof(ConstantType) });

            if (method == null)
                throw new NotSupportedException("Unrecognized type");
        }

        /// <summary>
        /// The constant.
        /// </summary>
        public ConstantType Constant { get; set; }
        
        /// <summary>
        /// The index for a constant value does not make sense.
        /// <see cref="IElement.Index"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">If the set accessor is invoked.</exception>
        public override IElement Index
        {
            get { return null; }
            set { throw new InvalidOperationException("Can't set an index for a constant value."); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override Type LoadedType
        {
            get { return typeof(ConstantType); }
            set { throw new InvalidOperationException("RootType for constant type is fixed."); }
        }

        public override ASIndexAttribute Attribute
        {
            get
            {
                return null;
            }
        }

        protected override void InternalStore(Emit g, TypeOfContent content)
        {
            throw new NotSupportedException("This element is constant.");
        }

        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            if (content == TypeOfContent.Address)
                throw new NotSupportedException("Requested the address of a constant value.");

            if (method == null)
                throw new InvalidOperationException("Unrecognized type");
            method.Invoke(g, new object[] { Constant });
        }
    }
}
