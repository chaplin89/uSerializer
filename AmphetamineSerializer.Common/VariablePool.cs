using AmphetamineSerializer.Common.Element;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmphetamineSerializer
{
    class VariableStatus
    {
        internal LocalElement Variable;
        internal bool IsFree;
    }

    public class VariablePool
    {
        private Emit g;
        Dictionary<Type, List<VariableStatus>> currentPool;

        public VariablePool(Emit emiter)
        {
            g = emiter;
            currentPool = new Dictionary<Type, List<VariableStatus>>();
        }

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

        public void ReleaseVariable(LocalElement element)
        {
            currentPool[element.LoadedType].Where(_1 => _1.Variable == element)
                                           .Single().IsFree = false;
        }
    }
}