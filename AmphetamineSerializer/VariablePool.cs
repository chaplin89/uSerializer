using AmphetamineSerializer.Common.Element;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmphetamineSerializer
{
    /// <summary>
    /// Manage a pool of variables in the stack.
    /// </summary>
    /// <remarks>
    /// Keep track of local variables used inside a function.
    /// When a variable is not used anymore, can be recycled in order to 
    /// avoid flooding the stack with unused variables.
    /// </remarks>
    public class VariablePool
    {
        class VariableStatus
        {
            internal LocalElement Variable;
            internal bool IsFree;
        }

        private Emit g;
        private Dictionary<Type, List<VariableStatus>> currentPool;

        /// <summary>
        /// Buld the pool.
        /// </summary>
        /// <param name="emiter">Emiter.</param>
        public VariablePool(Emit emiter)
        {
            g = emiter;
            currentPool = new Dictionary<Type, List<VariableStatus>>();
        }

        /// <summary>
        /// Allocate a new variable or return an unused variable.
        /// </summary>
        /// <param name="variableType">Type of the variable to allocate.</param>
        /// <returns>Element that represent the variable.</returns>
        public LocalElement GetNewVariable(Type variableType)
        {
            List<VariableStatus> currentList = null;
            VariableStatus currentVariable;

            if (!currentPool.TryGetValue(variableType, out currentList))
            {
                currentList = new List<VariableStatus>();
                currentPool.Add(variableType, currentList);
            }

            if (!currentList.Where(_1 => _1.IsFree).Any())
            {
                currentVariable = new VariableStatus();
                currentVariable.Variable = g.DeclareLocal(variableType);
                currentList.Add(currentVariable);
            }
            else
            {
                currentVariable = currentPool[variableType].First(_1 => _1.IsFree);
            }

            currentVariable.IsFree = false;
            return currentVariable.Variable;
        }

        /// <summary>
        /// Mark a variable as "not used" so that can be re-used subsequently.
        /// </summary>
        /// <param name="element">Element that represent the variable to release.</param>
        public void ReleaseVariable(LocalElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            currentPool[element.LoadedType].Where(_1 => _1.Variable == element)
                                           .Single().IsFree = false;
        }
    }
}