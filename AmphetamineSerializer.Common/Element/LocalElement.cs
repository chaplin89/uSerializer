using Sigil.NonGeneric;
using Sigil;
using AmphetamineSerializer.Model.Attributes;
using System;
using AmphetamineSerializer.Model;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage a local variable.
    /// </summary>
    public class LocalElement : BaseElement
    {
        /// <summary>
        /// Build a LocalElement wrapper around a Local variable.
        /// </summary>
        /// <param name="local">The local variable</param>
        public static implicit operator LocalElement(Local local)
        {
            return new LocalElement(local);
        }

        /// <summary>
        /// Convert this object to its contained variable.
        /// </summary>
        /// <param name="local">The local variable</param>
        public static implicit operator Local(LocalElement local)
        {
            return local.LocalVariable;
        }

        /// <summary>
        /// Build this object with a local variable.
        /// </summary>
        /// <param name="local">The local variable</param>
        public LocalElement(Local local)
        {
            LocalVariable = local;
            loadedType = local.LocalType;
        }

        /// <summary>
        /// The local variable
        /// </summary>
        public Local LocalVariable { get; set; }

        public override ASIndexAttribute Attribute
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="content"></param>
        protected override void InternalStore(Emit g, TypeOfContent content)
        {
            g.StoreLocal(LocalVariable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="content"></param>
        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            if (content == TypeOfContent.Value)
                g.LoadLocal(LocalVariable);
            else
                g.LoadLocalAddress(LocalVariable);
        }
    }
}
