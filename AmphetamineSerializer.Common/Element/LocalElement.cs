using System;
using Sigil.NonGeneric;
using Sigil;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage a local variable.
    /// </summary>
    public class LocalElement : BaseElement
    {
        private Type elementType;

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
        }

        /// <summary>
        /// The local variable
        /// </summary>
        public Local LocalVariable { get; set; }
        
        /// <summary>
        /// <see cref="IElement.LoadedType"/>
        /// </summary>
        public override Type LoadedType
        {
            get { return LocalVariable?.LocalType; }
            set { throw new InvalidOperationException("Can't set the RootType for LocalElement because it's fixed."); }
        }

        protected override void InternalStore(Emit g, TypeOfContent content)
        {
            g.StoreLocal(LocalVariable);
        }

        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            if (content == TypeOfContent.Value)
                g.LoadLocal(LocalVariable);
            else
                g.LoadLocalAddress(LocalVariable);
        }
    }
}
