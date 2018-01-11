using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model.Attributes;
using System;
using System.Reflection;

namespace AmphetamineSerializer.Common.Element
{
    public abstract class MemberElement : BaseElement
    {
        public MemberElement(IElement instance, MemberInfo member, Type loadedType)
        {
            Instance = instance;
            Member = member;
            base.loadedType = loadedType;
        }

        /// <summary>
        /// Object instance.
        /// </summary>
        public IElement Instance { get; private set; }

        /// <summary>
        /// Field information.
        /// </summary>
        public MemberInfo Member { get; private set; }
        
        /// <summary>
        /// Access the ASIndexAttribute of the field.
        /// </summary>
        public override ASIndexAttribute Attribute
        {
            get
            {
                if (Member == null)
                    return null;
                return Member.GetCustomAttribute<ASIndexAttribute>();
            }
        }
    }
}