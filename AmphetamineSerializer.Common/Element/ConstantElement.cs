using System;
using Sigil.NonGeneric;
using System.Reflection;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage the load of a constant value in the stack.
    /// </summary>
    /// <typeparam name="ConstantType">Type of constant value</typeparam>
    public class ConstantElement<ConstantType> : IElement
    {
        /// <summary>
        /// Build a ConstantElement wrapper arount a constant.
        /// </summary>
        /// <param name="constant">The constant</param>
        public static implicit operator ConstantElement<ConstantType>(ConstantType constant)
        {
            return new ConstantElement<ConstantType>(constant);
        }

        /// <summary>
        /// Return the constant contained in this object.
        /// </summary>
        /// <param name="constant">The constant</param>
        public static implicit operator ConstantType(ConstantElement<ConstantType> constant)
        {
            return constant.Constant;
        }

        MethodInfo method;

        /// <summary>
        /// Build a 
        /// </summary>
        /// <param name="constant"></param>
        public ConstantElement(ConstantType constant)
        {
            Constant = constant;
            method = typeof(Emit).GetMethod("LoadConstant", new Type[] { typeof(ConstantType) });

            if (method == null)
                throw new InvalidOperationException("Unrecognized type");
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
        public Action<Emit, TypeOfContent> Load
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
        /// Not supported because a constant value can't be modified.
        /// </summary>
        /// <exception cref="NotSupportedException">Always</exception>
        public Action<Emit, IElement, TypeOfContent> Store
        {
            get
            {
                throw new NotSupportedException("Trying to set a constant value.");
            }
        }
    }
}
