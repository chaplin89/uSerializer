using System;
using Sigil.NonGeneric;
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
        /// Emit instructions to load the value in the stack.
        /// </summary>
        /// <exception cref="NotSupportedException">If the content is requested by address</exception>
        /// <exception cref="InvalidOperationException">If the method for emitting the constant was not found.</exception>
        public override Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (content == TypeOfContent.Address)
                        throw new NotSupportedException("Requested the address of a constant value.");

                    if (method == null)
                        throw new InvalidOperationException("Unrecognized type");
                    method.Invoke(g, new object[] { Constant });
                };
            }
        }
        
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
        /// Return the type of the element.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the set accessor is invoked.</exception>
        public override Type ElementType
        {
            get { return RootType; }
            set { throw new InvalidOperationException("Element type for a constant value is fixed."); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Type RootType
        {
            get { return typeof(ValueType); }
            set { throw new InvalidOperationException("RootType for constant type is fixed."); }
        }

        protected override Action<Emit, IElement, TypeOfContent> InternalStore(IElement index)
        {
            throw new NotImplementedException();
        }

        protected override Action<Emit, TypeOfContent> InternalLoad(IElement index)
        {
            throw new NotImplementedException();
        }
    }
}
