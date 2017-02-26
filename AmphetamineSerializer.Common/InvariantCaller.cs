using AmphetamineSerializer.Chain;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class InvariantCaller
    {
        Type[] availableParameters;
        ushort?[] parameterOrder;
        
        public InvariantCaller()
        {
        }


        public void SetInput(Type[] availableParameters)
        {
            this.availableParameters = availableParameters;
        }
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptors"></param>
        /// <returns></returns>
        public bool SetOutput(ParameterDescriptor[] descriptors)
        {
            var mandatory = descriptors.Where(_=> _.Role == Attributes.ParameterRole.MandatoryForward);
            var optional = descriptors.Where(_ => _.Role == Attributes.ParameterRole.OptionalForward);
            var rootType = descriptors.Where(_ => _.Role == Attributes.ParameterRole.RootObject);

            if (rootType.Count() != 1)
                throw new InvalidOperationException("RootObject count is not 1.");

            if (!BindSubset(rootType, parameterOrder) ||
                !BindSubset(mandatory, parameterOrder))
            {
                parameterOrder = null;
                return false;
            }

            BindSubset(optional, parameterOrder);
            return true;
        }

        public void EmitInvoke(Emit emiter)
        {
            if (parameterOrder == null)
                throw new InvalidOperationException("Bind the parameters first.");

            // TODO: manage missing parameter.
            foreach (var item in parameterOrder)
            {
                Debug.Assert(item.HasValue);
                emiter.LoadArgument(item.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        public void Invoke(MethodInfo method, object[] input, object instance = null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="foundIndexes"></param>
        /// <returns>True if all parameters were binded successfully.</returns>
        private bool BindSubset(IEnumerable<ParameterDescriptor> parameters, ushort?[] foundIndexes)
        {
            bool completed = true;
            for (ushort i = 0; i < availableParameters.Length; ++i)
            {
                foreach (var item in parameters)
                {
                    if (foundIndexes[item.Index] != null)
                        continue;

                    completed = false;

                    if (item.Parameter == availableParameters[i])
                        foundIndexes[item.Index] = i;
                }

                if (completed)
                    break;

                completed = true;
            }

            return !parameters.Where(_ => !foundIndexes.Contains((ushort)_.Index)).Any();
        }
    }
}
