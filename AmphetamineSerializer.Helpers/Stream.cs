using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    public class StreamDeserializationCtx : IBuilder
    {
        static StreamDeserializationCtx()
        {
            // Not all methods were found
            Debug.Assert(typeHandlerMap.Where(x => x.Value == null).Count() == 0);
        }

        private FoundryContext ctx;
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

            {typeof(byte).MakeByRefType(),   typeof(BinaryReader).GetMethod("ReadByte")},
            {typeof(sbyte).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadSByte")},
            {typeof(uint).MakeByRefType(),   typeof(BinaryReader).GetMethod("ReadUInt32")},
            {typeof(int).MakeByRefType(),    typeof(BinaryReader).GetMethod("ReadInt32")},
            {typeof(ushort).MakeByRefType(), typeof(BinaryReader).GetMethod("ReadUInt16")},
            {typeof(short).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadInt16")},
            {typeof(double).MakeByRefType(), typeof(BinaryReader).GetMethod("ReadDouble")},
            {typeof(float).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadSingle")},
            {typeof(byte[]).MakeByRefType(),  typeof(BinaryReader).GetMethod("ReadBytes")}
        };
        private BuildedFunction function;

        public StreamDeserializationCtx(FoundryContext ctx)
        {
            this.ctx = ctx;
        }

        public BuildedFunction Make()
        {
            if (function != null)
                return function;

            if (ctx.Element.UnderlyingType == null)
                return null;

            if (ctx.Element.UnderlyingType == typeof(string))
                HandleString(ctx);
            else if (typeHandlerMap.ContainsKey(ctx.Element.UnderlyingType))
                HandlePrimitive(ctx);
            else
                return null;

            function = new BuildedFunction() { Status = BuildedFunctionStatus.NoMethodsAvailable };
            return function;
        }

        [SerializationHandler(typeof(string))]
        public void HandleString(FoundryContext ctx)
        {
            if (ctx.ManageLifeCycle)
                DecodeString(ctx);
            else
                EncodeString(ctx);
        }

        public void DecodeString(FoundryContext ctx)
        {
            // Rough C# translation:
            // Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));

            ctx.Manipulator.Store(ctx, (context) =>
            {
                // Put the decoded string in the stack.
                context.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                context.G.LoadArgument(1);
                context.G.LoadArgument(1);
                context.G.CallVirtual(typeHandlerMap[typeof(int).MakeByRefType()]);
                context.G.CallVirtual(typeHandlerMap[typeof(byte[]).MakeByRefType()]);
                context.G.CallVirtual(typeof(Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }));
            });
        }

        public BuildedFunction EncodeString(FoundryContext ctx)
        {
            // writer.Write(Encoding.ASCII.GetByteCount(Load()));
            // writer.Write(Encoding.ASCII.GetBytes(Load());

            // Write lenght
            {
                ctx.G.LoadArgument(1);
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                ctx.Manipulator.Load(ctx);
                ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) }));
                ctx.G.CallVirtual(typeHandlerMap[typeof(int)]);
            }
            // Write string
            {
                ctx.G.LoadArgument(1);
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                ctx.Manipulator.Load(ctx);
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
                ctx.Manipulator.Store(ctx, (context) =>
                {
                    context.G.LoadArgument(1); // argument i --> stack                
                    ctx.G.CallVirtual(typeHandlerMap[ctx.NormalizedType]);
                });
            }
            else
            {
                ctx.G.LoadArgument(1); // argument i --> stack
                ctx.Manipulator.Load(ctx);
                ctx.G.CallVirtual(typeHandlerMap[ctx.NormalizedType]);
            }
        }
    }
}
