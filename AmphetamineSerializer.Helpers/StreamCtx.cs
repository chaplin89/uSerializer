using AmphetamineSerializer.Common;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AmphetamineSerializer.Helpers
{
    static public class StreamDeserializationCtx
    {
        static private readonly Dictionary<Type, MethodInfo> typeHandlerMap;

        static StreamDeserializationCtx()
        {
            Type tr = typeof(BinaryReader);
            Type tw = typeof(BinaryWriter);
            typeHandlerMap = new Dictionary<Type, MethodInfo>()
            {
                {typeof(byte),   tw.GetMethod("Write", new Type[] {typeof(byte),   })},
                {typeof(sbyte),  tw.GetMethod("Write", new Type[] {typeof(sbyte),  })},
                {typeof(uint),   tw.GetMethod("Write", new Type[] {typeof(uint),   })},
                {typeof(int),    tw.GetMethod("Write", new Type[] {typeof(int),    })},
                {typeof(ushort), tw.GetMethod("Write", new Type[] {typeof(ushort), })},
                {typeof(short),  tw.GetMethod("Write", new Type[] {typeof(short),  })},
                {typeof(double), tw.GetMethod("Write", new Type[] {typeof(double), })},
                {typeof(float),  tw.GetMethod("Write", new Type[] {typeof(float),  })},

                {typeof(byte).MakeByRefType(),   tr.GetMethod("ReadByte")},
                {typeof(sbyte).MakeByRefType(),  tr.GetMethod("ReadSByte")},
                {typeof(uint).MakeByRefType(),   tr.GetMethod("ReadUInt32")},
                {typeof(int).MakeByRefType(),    tr.GetMethod("ReadInt32")},
                {typeof(ushort).MakeByRefType(), tr.GetMethod("ReadUInt16")},
                {typeof(short).MakeByRefType(),  tr.GetMethod("ReadInt16")},
                {typeof(double).MakeByRefType(), tr.GetMethod("ReadDouble")},
                {typeof(float).MakeByRefType(),  tr.GetMethod("ReadFloat")},
            };
        }

        [SerializationHandler(typeof(string))]
        static public Emit DecodeString(FoundryContext ctx)
        {
            Type currentType = ctx.CurrentItemUnderlyingType.MakeByRefType();
            if (ctx.Provider.AlreadyBuildedMethods.ContainsKey(currentType))
                return ctx.Provider.AlreadyBuildedMethods[currentType];

            List<Type> input = new List<Type>() { currentType };
            for (int i = 1; i < ctx.InputParameters.Length; i++)
                input.Add(ctx.InputParameters[i]);

            Emit e = ctx.Provider.AddMethod("Read", input.ToArray(), typeof(void));
            e.Return();
            return ctx.Provider.GetEmit(false);
        }

        [SerializationHandler(typeof(byte))]
        [SerializationHandler(typeof(sbyte))]
        [SerializationHandler(typeof(uint))]
        [SerializationHandler(typeof(int))]
        [SerializationHandler(typeof(ushort))]
        [SerializationHandler(typeof(short))]
        [SerializationHandler(typeof(double))]
        [SerializationHandler(typeof(float))]
        static public Emit HandlePrimitive(FoundryContext ctx)
        {
            if (ctx.ObjectType.IsByRef)
                return HandleRead(ctx);
            else
                return HandleWrite(ctx);
        }

        private static Emit HandleRead(FoundryContext ctx)
        {
            Type currentType = ctx.CurrentItemUnderlyingType.MakeByRefType();
            if (ctx.Provider.AlreadyBuildedMethods.ContainsKey(currentType))
                return ctx.Provider.AlreadyBuildedMethods[currentType];

            if (!typeHandlerMap.ContainsKey(currentType))
                return null;

            List<Type> input = new List<Type>() { currentType };
            for (int i = 1; i < ctx.InputParameters.Length; i++)
                input.Add(ctx.InputParameters[i]);

            Emit e = ctx.Provider.AddMethod("Read", input.ToArray(), typeof(void));
            e.LoadArgument(1); // argument i --> stack
            e.CallVirtual(typeHandlerMap[currentType]);
            e.StoreArgument(0);
            e.Return();
            return ctx.Provider.GetEmit(false);
        }

        private static Emit HandleWrite(FoundryContext ctx)
        {
            Type currentType = ctx.CurrentItemUnderlyingType;
            if (ctx.Provider.AlreadyBuildedMethods.ContainsKey(currentType))
                return ctx.Provider.AlreadyBuildedMethods[currentType];

            if (!typeHandlerMap.ContainsKey(currentType))
                return null;

            List<Type> input = new List<Type>() { currentType };
            for (int i = 1; i < ctx.InputParameters.Length; i++)
                input.Add(ctx.InputParameters[i]);

            Emit e = ctx.Provider.AddMethod("Write", input.ToArray(), typeof(void));
            e.LoadArgument(1); // argument i --> stack
            e.LoadArgument(0); // argument i --> stack
            e.CallVirtual(typeHandlerMap[currentType]);
            e.Return();
            return ctx.Provider.GetEmit(false);

        }
    }
}
