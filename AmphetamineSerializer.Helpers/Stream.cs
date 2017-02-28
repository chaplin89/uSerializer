using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    public class StreamDeserializationCtx : BuilderBase
    {
        /// <summary>
        /// Map every trivial type with its handler inside BinaryWriter or BinaryReader.
        /// </summary>
        static private readonly Dictionary<Type, MethodInfo> typeHandlerMap = new Dictionary<Type, MethodInfo>()
        {
            {typeof(byte),                   typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(byte),   })},
            {typeof(sbyte),                  typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(sbyte),  })},
            {typeof(uint),                   typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(uint),   })},
            {typeof(int),                    typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(int),    })},
            {typeof(ushort),                 typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(ushort), })},
            {typeof(short),                  typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(short),  })},
            {typeof(double),                 typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(double), })},
            {typeof(float),                  typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(float),  })},
            {typeof(byte[]),                 typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(byte[]), })},
            {typeof(ulong),                  typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(ulong),  })},
            {typeof(long),                   typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(long),   })},
            {typeof(char),                   typeof(BinaryWriter).GetMethod("Write", new Type[] {typeof(char),   })},

            {typeof(byte).MakeByRefType(),   typeof(BinaryReader).GetMethod("ReadByte")},
            {typeof(sbyte).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadSByte")},
            {typeof(uint).MakeByRefType(),   typeof(BinaryReader).GetMethod("ReadUInt32")},
            {typeof(int).MakeByRefType(),    typeof(BinaryReader).GetMethod("ReadInt32")},
            {typeof(ushort).MakeByRefType(), typeof(BinaryReader).GetMethod("ReadUInt16")},
            {typeof(short).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadInt16")},
            {typeof(double).MakeByRefType(), typeof(BinaryReader).GetMethod("ReadDouble")},
            {typeof(float).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadSingle")},
            {typeof(byte[]).MakeByRefType(), typeof(BinaryReader).GetMethod("ReadBytes")},
            {typeof(ulong).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadUInt64")},
            {typeof(long).MakeByRefType(),   typeof(BinaryReader).GetMethod("ReadInt64")},
            {typeof(char).MakeByRefType(),   typeof(BinaryReader).GetMethod("ReadChar")},
        };

        InvariantCaller caller = new InvariantCaller();

        static StreamDeserializationCtx()
        {
            // Not all methods were found
            Debug.Assert(typeHandlerMap.Where(x => x.Value == null).Count() == 0);
        }

        public StreamDeserializationCtx(FoundryContext ctx) : base(ctx)
        {
            caller.SetInput(ctx.InputParameters);
        }

        protected override BuildedFunction InternalMake()
        {
            if (ctx.Element.ElementType == null)
                return null;

            if (typeHandlerMap.ContainsKey(ctx.Element.ElementType))
                HandlePrimitive(ctx);
            if (ctx.Element.ElementType == typeof(string))
                HandleString(ctx);
            else
                return null;

            method = new BuildedFunction() { Status = BuildedFunctionStatus.NoMethodsAvailable };
            return method;
        }

        [SerializationHandler(typeof(string))]
        public void HandleString(FoundryContext ctx)
        {
            if (ctx.ManageLifeCycle)
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
        public void DecodeString(FoundryContext ctx)
        {
            var store = (GenericElement)((g, _) =>
            {
                // Put the decoded string in the stack.
                g.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                g.LoadArgument(1);
                g.LoadArgument(1);
                g.CallVirtual(typeHandlerMap[typeof(int).MakeByRefType()]);
                g.CallVirtual(typeHandlerMap[typeof(byte[]).MakeByRefType()]);
                g.CallVirtual(typeof(Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }));
            });

            ctx.Element.Store(ctx.G, store, TypeOfContent.Value);
        }

        public BuildedFunction EncodeString(FoundryContext ctx)
        {
            // Rough C# translation:
            // writer.Write(Encoding.ASCII.GetByteCount(Load()));
            // writer.Write(Encoding.ASCII.GetBytes(Load());

            // Write lenght
            {
                ctx.G.LoadArgument(1);
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                ctx.Element.Load(ctx.G, TypeOfContent.Value);
                ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) }));
                ctx.G.CallVirtual(typeHandlerMap[typeof(int)]);
            }
            // Write string
            {
                ctx.G.LoadArgument(1);
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                ctx.Element.Load(ctx.G, TypeOfContent.Value);
                ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetBytes", new Type[] { typeof(string) }));
                ctx.G.CallVirtual(typeHandlerMap[typeof(byte[])]);
            }

            return new BuildedFunction() { Status = BuildedFunctionStatus.NoMethodsAvailable };
        }

        [SerializationHandler(typeof(byte))]
        [SerializationHandler(typeof(sbyte))]
        [SerializationHandler(typeof(uint))]
        [SerializationHandler(typeof(int))]
        [SerializationHandler(typeof(ushort))]
        [SerializationHandler(typeof(short))]
        [SerializationHandler(typeof(double))]
        [SerializationHandler(typeof(float))]
        public void HandlePrimitive(FoundryContext ctx)
        {
            if (ctx.ManageLifeCycle)
            {
                var store = (GenericElement)((g, _) =>
                {
                    caller.SetOutput(new ParameterDescriptor[] 
                    {
                        new ParameterDescriptor()
                        {
                            Index = 0,
                            Parameter = typeof(BinaryReader),
                            Role = ParameterRole.MandatoryForward
                        }
                    });
                    caller.EmitInvoke(g);
                    g.CallVirtual(typeHandlerMap[ctx.Element.ElementType]);
                });
            }
            else
            {
                caller.SetOutput(new ParameterDescriptor[]
                {
                    new ParameterDescriptor()
                    {
                        Index = 0,
                        Parameter = typeof(BinaryWriter),
                        Role = ParameterRole.MandatoryForward
                    }
                });
                caller.EmitInvoke(ctx.G);
                ctx.G.LoadArgument(1); // argument i --> stack
                ctx.Element.Load(ctx.G, TypeOfContent.Value);
                ctx.G.CallVirtual(typeHandlerMap[ctx.NormalizedType]);
            }
        }
    }
}
