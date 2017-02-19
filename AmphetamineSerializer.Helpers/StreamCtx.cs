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

        public StreamDeserializationCtx(FoundryContext ctx)
        {
            this.ctx = ctx;
        }

        public BuildedFunction Method
        {
            get
            {
                if (ctx.CurrentItemUnderlyingType == null)
                    return null;

                if (ctx.CurrentItemUnderlyingType == typeof(string))
                    HandleString(ctx);
                else if (typeHandlerMap.ContainsKey(ctx.CurrentItemUnderlyingType))
                    HandlePrimitive(ctx);
                else
                    return null;

                return new BuildedFunction() { Status = BuildedFunctionStatus.NoMethodsAvailable };
            }
        }

        private void Load()
        {
            if (ctx.CurrentItemType.IsArray)
            {
                ctx.G.LoadLocal(ctx.ObjectInstance);
                ctx.G.LoadField(ctx.CurrentItemFieldInfo);
                ctx.G.LoadLocal(ctx.Index);
                ctx.G.LoadElement(ctx.CurrentItemUnderlyingType);
            }
            else
            {
                ctx.G.LoadLocal(ctx.ObjectInstance);
                ctx.G.LoadField(ctx.CurrentItemFieldInfo);
            }
        }

        void BeginStore()
        {
            if (ctx.CurrentItemType.IsArray)
            {
                ctx.G.LoadLocal(ctx.ObjectInstance);
                ctx.G.LoadField(ctx.CurrentItemFieldInfo);
                ctx.G.LoadLocal(ctx.Index);
            }
            else
            {
                ctx.G.LoadLocal(ctx.ObjectInstance);
            }
        }

        void EndStore()
        {
            if (ctx.CurrentItemType.IsArray)
                ctx.G.StoreElement(ctx.CurrentItemUnderlyingType);
            else
                ctx.G.StoreField(ctx.CurrentItemFieldInfo);
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

            // Encoding.ASCII instance (Encoder#1)
            // BinaryReader (Reader#1)
            // Duplicate (Reader#2)
            // ReadInt(Reader#2) -> Size in stack
            // ReadBytes(Reader#1, Size) -> bytes in stack
            // Encoding.ASCII.GetString(Encoder#1, bytes)

            // Put the decoded string in the stack.
            BeginStore();
            {
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                ctx.G.LoadArgument(1);
                ctx.G.LoadArgument(1);
                ctx.G.CallVirtual(typeHandlerMap[typeof(int).MakeByRefType()]);
                ctx.G.CallVirtual(typeHandlerMap[typeof(byte[]).MakeByRefType()]);
                ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }));
            }
            EndStore();
        }

        public BuildedFunction EncodeString(FoundryContext ctx)
        {
            // writer.Write(Encoding.ASCII.GetByteCount(Load()));
            // writer.Write(Encoding.ASCII.GetBytes(Load());

            // Write lenght
            {
                ctx.G.LoadArgument(1);
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                Load();
                ctx.G.CallVirtual(typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) }));
                ctx.G.CallVirtual(typeHandlerMap[typeof(int)]);
            }
            // Write string
            {
                ctx.G.LoadArgument(1);
                ctx.G.Call(typeof(Encoding).GetProperty("ASCII").GetMethod);
                Load();
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
                BeginStore();

            ctx.G.LoadArgument(1); // argument i --> stack

            if (!ctx.ManageLifeCycle)
                Load();

            ctx.G.CallVirtual(typeHandlerMap[ctx.NormalizedType]);

            if (ctx.ManageLifeCycle)
                EndStore();
        }
    }
}
