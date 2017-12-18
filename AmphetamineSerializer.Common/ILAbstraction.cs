using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using Sigil.NonGeneric;
using Sigil;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Model.Attributes;
using AmphetamineSerializer.Common.Element;

namespace AmphetamineSerializer
{
    /// <summary>
    /// Provide basic abstraction from IL.
    /// TODO: REMOVE THIS MESS!!
    /// </summary>
    public class ILAbstraction
    {
        private Emit g;

        /// <summary>
        /// Construct an ILAbstraction object.
        /// </summary>
        /// <param name="g">Generator</param>
        public ILAbstraction(Emit g)
        {
            this.g = g;
        }

        /// <summary>
        /// Increment a local variable with a given step
        /// </summary>
        /// <param name="index">Variable to increment</param>
        /// <param name="step">Step</param>
        public void IncrementLocalVariable(Local index, int step = 1)
        {
            // C# Translation:
            //     index+=step;
            g.LoadLocal(index); // index --> stack
            if (step == 1)
                g.LoadConstant(1); // 1 --> stack
            else
                g.LoadConstant(step); // step --> stack
            g.Add(); // index + step --> stack
            g.StoreLocal(index); // stack --> index
        }

        public Type MakeDelegateType(Type objectType, Type[] inputTypes)
        {
            List<Type> arguments = new List<Type>(inputTypes.Length + 1);
            arguments.Add(objectType);

            for (int i = 1; i < inputTypes.Length; i++)
                arguments.Add(inputTypes[i]);
            arguments.Add(typeof(void));

            return Expression.GetDelegateType(arguments.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        public void ForwardParameters(Type[] inputParameter, BuildedFunction currentMethod, ASIndexAttribute attribute = null)
        {
            if (currentMethod != null && currentMethod.Status == BuildedFunctionStatus.TypeFinalized)
            {
                ParameterInfo[] parameters = currentMethod.Method.GetParameters();
                bool[] foundParameter = new bool[parameters.Length - 1];

                for (int i = 1; i < parameters.Length; ++i)
                {
                    for (ushort j = 1; j < inputParameter.Length; j++)
                    {
                        if (inputParameter[j] == parameters[i].ParameterType)
                        {
                            if (foundParameter[i - 1])
                                throw new AmbiguousMatchException("Input arguments match more than one argument in the handler signature.");
                            foundParameter[i - 1] = true;

                            g.LoadArgument(j); // argument i --> stack
                            break;
                        }
                    }
                    if (!foundParameter[i - 1])
                    {
                        throw new InvalidOperationException("Unable to load all the parameters for the handler.");
                    }
                }

                if (currentMethod.Method.IsStatic)
                    g.Call(currentMethod.Method);                 // void func(ref obj,byte[], ref int)
                else
                    g.CallVirtual(currentMethod.Method);
            }
            else
            {
                for (ushort j = 1; j < inputParameter.Length; j++)
                    g.LoadArgument(j); // argument i --> stack
            }
        }
    }
}
