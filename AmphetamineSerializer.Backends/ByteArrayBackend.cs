using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Common.Element;
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
        /// <summary>
        /// Map every trivial type with its handler inside BinaryWriter or BinaryReader.
        /// </summary>
        static private readonly Dictionary<Type, Tuple<MethodInfo, uint>> typeHandlerMap = new Dictionary<Type, Tuple<MethodInfo, uint>>()
        {
            // From obj to byte[]
           {typeof(uint),                   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(uint),   }), 4u)},
           {typeof(int),                    new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(int),    }), 4u)},
           {typeof(ushort),                 new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(ushort), }), 2u)},
           {typeof(short),                  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(short),  }), 2u)},
           {typeof(double),                 new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(double), }), 8u)},
           {typeof(float),                  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(float),  }), 4u)},
           {typeof(ulong),                  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(ulong),  }), 8u)},
           {typeof(long),                   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(long),   }), 8u)},
           {typeof(char),                   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("GetBytes", new Type[] {typeof(char),   }), 1u)},

           // From byte[] to Obj
           {typeof(uint).MakeByRefType(),   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToUInt32")                                ,4u)  },
           {typeof(int).MakeByRefType(),    new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToInt32")                                 ,4u)  },
           {typeof(ushort).MakeByRefType(), new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToUInt16")                                ,2u)  },
           {typeof(short).MakeByRefType(),  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToInt16")                                 ,2u)  },
           {typeof(double).MakeByRefType(), new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToDouble")                                ,8u)  },
           {typeof(float).MakeByRefType(),  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToSingle")                                ,4u)  },
           {typeof(ulong).MakeByRefType(),  new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToUInt64")                                ,8u)  },
           {typeof(long).MakeByRefType(),   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToInt64")                                 ,8u)  },
           {typeof(char).MakeByRefType(),   new Tuple<MethodInfo,uint>(typeof(BitConverter).GetMethod("ToChar")                                  ,1u)  },
        };

        static ByteArrayBackend()
        {
            // Not all methods were found
            Debug.Assert(!typeHandlerMap.Where(x => x.Value == null).Any());
        }

        public ByteArrayBackend(Context ctx) : base(ctx)
        {
        }

        protected override ElementBuildResponse InternalMake()
        {
            if (ctx.CurrentElement?.LoadedType == null)
                return null;

            if (typeHandlerMap.ContainsKey(ctx.CurrentElement.LoadedType))
                HandlePrimitive(ctx);
            else if (ctx.CurrentElement.LoadedType == typeof(string))
                HandleString(ctx);
            else
                return null;

            method = new ElementBuildResponse() { Status = BuildedFunctionStatus.ContextModified };
            return method;
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

        public ElementBuildResponse EncodeString(Context ctx)
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
            return new ElementBuildResponse() { Status = BuildedFunctionStatus.ContextModified };
        }

        public void HandlePrimitive(Context ctx)
        {
            if (ctx.IsDeserializing)
            {
                //Read from stream
                var readFromStream = new GenericElement(((g, _) =>
                {
                    ctx.G.LoadArgument(1);
                    ctx.G.LoadArgument(2);
                    ctx.G.LoadIndirect(typeof(uint));

                    g.Call(typeHandlerMap[ctx.CurrentElement.LoadedType.MakeByRefType()].Item1);
                }), null);

                ctx.CurrentElement.Store(ctx.G, readFromStream, TypeOfContent.Value);

                ctx.G.LoadArgument(2);
                ctx.G.LoadArgument(2);
                ctx.G.LoadIndirect(typeof(uint));
                ctx.G.LoadConstant(typeHandlerMap[ctx.CurrentElement.LoadedType.MakeByRefType()].Item2);
                ctx.G.Add();
                ctx.G.StoreIndirect(typeof(uint));
            }
            else
            {
                //Write into stream
                ctx.G.LoadArgument(1);
                ctx.CurrentElement.Load(ctx.G, TypeOfContent.Value);
                ctx.G.Call(typeHandlerMap[ctx.CurrentElement.LoadedType].Item1);
            }
        }
    }
}
