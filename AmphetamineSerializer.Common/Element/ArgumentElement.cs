using AmphetamineSerializer.Model;
using AmphetamineSerializer.Model.Attributes;
using Sigil.NonGeneric;
using System;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage an argument.
    /// </summary>
    public class ArgumentElement : BaseElement
    {
        /// <summary>
        /// Build a wrapper around an argument index.
        /// </summary>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="argumentType">Type of the argument (no way to infer this).</param>
        public ArgumentElement(ushort argumentIndex, Type argumentType)
        {
            ArgumentIndex = argumentIndex;
            loadedType = argumentType;
        }

        /// <summary>
        /// Index of the argument
        /// </summary>
        public ushort ArgumentIndex { get; set; }

        public override ASIndexAttribute Attribute
        {
            get
            {
                return null;
            }
        }

        protected override void InternalLoad(Emit emit, TypeOfContent value)
        {
            if (value == TypeOfContent.Value)
                emit.LoadArgument(ArgumentIndex);
            else
                emit.LoadArgumentAddress(ArgumentIndex);
        }

        protected override void InternalStore(Emit emit, TypeOfContent content)
        {
            if (content == TypeOfContent.Address)
                emit.StoreIndirect(loadedType);
            else
                emit.StoreArgument(ArgumentIndex);
        }
    }
}
