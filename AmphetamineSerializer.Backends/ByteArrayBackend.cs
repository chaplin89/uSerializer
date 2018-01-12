using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Common.Element;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AmphetamineSerializer.Backends
{
    public class ByteArrayBackend : BuilderBase
    {
        IElement indexElement = new ArgumentElement(2, typeof(int).MakeByRefType());
        IElement byteArrayElement = new ArgumentElement(1, typeof(byte[]));

        IElement arrayWithOffset;

        /// <summary>
        /// Map every trivial type with its handler inside BitConverter.
        /// </summary>
        static private readonly Dictionary<Type, Tuple<MethodInfo, uint>> typeHandlerMap = new Dictionary<Type, Tuple<MethodInfo, uint>>()
        {
            // From obj to byte[]
           {typeof(uint),                   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(uint),   }), sizeof(uint) )},
           {typeof(int),                    new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(int),    }), sizeof(int)  )},
           {typeof(ushort),                 new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(ushort), }), sizeof(ushort))},
           {typeof(short),                  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(short),  }), sizeof(short))},
           {typeof(double),                 new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(double), }), sizeof(double))},
           {typeof(float),                  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(float),  }), sizeof(float))},
           {typeof(ulong),                  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(ulong),  }), sizeof(ulong))},
           {typeof(long),                   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(long),   }), sizeof(long) )},
           {typeof(char),                   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(char),   }), sizeof(char) )},

           // From byte[] to Obj
           {typeof(uint).MakeByRefType(),   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToUInt32")                                ,sizeof(uint) )  },
           {typeof(int).MakeByRefType(),    new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToInt32")                                 ,sizeof(int)  )  },
           {typeof(ushort).MakeByRefType(), new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToUInt16")                                ,sizeof(ushort)) },
           {typeof(short).MakeByRefType(),  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToInt16")                                 ,sizeof(short))  },
           {typeof(double).MakeByRefType(), new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToDouble")                                ,sizeof(double)) },
           {typeof(float).MakeByRefType(),  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToSingle")                                ,sizeof(float))  },
           {typeof(ulong).MakeByRefType(),  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToUInt64")                                ,sizeof(ulong))  },
           {typeof(long).MakeByRefType(),   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToInt64")                                 ,sizeof(long) )  },
           {typeof(char).MakeByRefType(),   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToChar")                                  ,sizeof(char) )  },
        };

        static ByteArrayBackend()
        {
            // Not all methods were found
            Debug.Assert(!typeHandlerMap.Where(x => x.Value == null).Any());
        }

        public ByteArrayBackend(Context ctx) : base(ctx)
        {
            arrayWithOffset = byteArrayElement.EnterArray(indexElement);
        }

        protected override IResponse InternalMake()
        {
            if (ctx.CurrentElement?.LoadedType == null)
                return null;

            if (ctx.InputParameters.Length != 3)
                return null;
            
            // if (ctx.AdditionalContext == null)
            // {
            //     var defaultFinder = new DefaultHandlerFinder().Use<ByteCountBackend>();
            //     var chain = new ChainManager().SetNext(defaultFinder);
            // 
            //     var oldChain = ctx.Chain;
            //     ctx.Chain = chain;
            // 
            //     var request = new ElementBuildRequest()
            //     {
            //         Element = ctx.CurrentElement,
            //         G = ctx.G,
            //         InputTypes = ctx.InputParameters,
            //         Provider = ctx.Provider
            //     };
            // 
            //     var response = ctx.Chain.Process(request);
            // }

            if (typeHandlerMap.ContainsKey(ctx.CurrentElement.LoadedType))
                HandlePrimitive(ctx);
            else if (ctx.CurrentElement.LoadedType == typeof(string))
                HandleString(ctx);
            else
                return null;
            
            return new ContextModifiedBuildResponse();
        }

        public void HandleString(Context ctx)
        {
            if (ctx.IsDeserializing)
                DecodeString(ctx);
            else
                EncodeString(ctx);
        }


        /// <summary>
        /// Rough C# translation:
        /// Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
        /// 
        /// One day this will support multiple encoding depending on attributes.
        /// </summary>
        /// <param name="ctx"></param>
        public void DecodeString(Context ctx)
        {
            var valueToLoad = new GenericElement(((g, _) =>
            {
                // Put the decoded string in the stack.
                g.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                g.LoadArgument(1);
                g.LoadArgument(1);
                g.CallVirtual(typeHandlerMap[typeof(int).MakeByRefType()].Item1);
                g.CallVirtual(typeHandlerMap[typeof(byte[]).MakeByRefType()].Item1);
                g.CallVirtual(typeof(Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }));
            }), null);

            ctx.CurrentElement.Store(ctx.G, valueToLoad, TypeOfContent.Value);
        }

        public ContextModifiedBuildResponse EncodeString(Context ctx)
        {
            // Rough C# translation:
            // writer.Write(Encoding.ASCII.GetByteCount(Load()));
            // writer.Write(Encoding.ASCII.GetBytes(Load());

            // Write lenght
            ctx.G.LoadArgument(1);
            ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
            ctx.CurrentElement.Load(ctx.G, TypeOfContent.Value);
            ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) }));
            ctx.G.CallVirtual(typeHandlerMap[typeof(int)].Item1);

            // Write string
            ctx.G.LoadArgument(1);
            ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
            ctx.CurrentElement.Load(ctx.G, TypeOfContent.Value);
            ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetBytes", new Type[] { typeof(string) }));
            ctx.G.CallVirtual(typeHandlerMap[typeof(byte[])].Item1);
            return new ContextModifiedBuildResponse();
        }

        public void HandlePrimitive(Context ctx)
        {
            if (ctx.IsDeserializing)
            {
                Type currentType = ctx.CurrentElement.LoadedType.MakeByRefType();
                ctx.CurrentElement.Store(ctx.G, GetReadElement(currentType), TypeOfContent.Value);
                
                var indexPlusSize = new GenericElement(((g, _) =>
                {
                    indexElement.Load(g, TypeOfContent.Value);
                    ctx.G.LoadConstant(typeHandlerMap[ctx.CurrentElement.LoadedType.MakeByRefType()].Item2);
                    ctx.G.Add();
                }), null);

                indexElement.Store(ctx.G, indexPlusSize, TypeOfContent.Value);
            }
            else
            {
                var typeTuple = typeHandlerMap[ctx.CurrentElement.LoadedType];

                //Write into stream
                ctx.G.LoadArgument(1);
                ctx.CurrentElement.Load(ctx.G, TypeOfContent.Value);
                ctx.G.Call(typeTuple.Item1);
            }
        }

        private IElement GetReadElement(Type type)
        {
            return new GenericElement(((g, _) =>
            {
                byteArrayElement.Load(g, TypeOfContent.Value);
                indexElement.Load(g, TypeOfContent.Value);
                g.Call(typeHandlerMap[type].Item1);
            }), null);
        }
    }
}
