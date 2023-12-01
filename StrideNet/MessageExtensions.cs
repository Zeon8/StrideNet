using MemoryPack;
using Riptide;
using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;

namespace StrideNet
{
    public static class MessageExtensions
    {
        private static readonly Dictionary<Type, Func<Message, object>> s_getValue = new()
        {
            { typeof(bool), message => message.GetBool() },
            { typeof(byte), message => message.GetByte() },
            { typeof(short), message => message.GetShort() },
            { typeof(int), message => message.GetInt() },
            { typeof(uint), message => message.GetUInt() },
            { typeof(long), message => message.GetLong() },
            { typeof(ulong), message => message.GetULong()   },
            { typeof(float), message => message.GetFloat() },
            { typeof(double), message => message.GetDouble() },
            { typeof(string), message => message.GetString() },

            { typeof(bool[]), message => message.GetBools() },
            { typeof(byte[]), message => message.GetBytes() },
            { typeof(short[]), message => message.GetShorts() },
            { typeof(int[]), message => message.GetInts() },
            { typeof(uint[]), message => message.GetUInts() },
            { typeof(long[]), message => message.GetLongs() },
            { typeof(ulong[]), message => message.GetULongs()   },
            { typeof(float[]), message => message.GetFloats() },
            { typeof(double[]), message => message.GetDoubles() },
            { typeof(string[]), message => message.GetStrings() },

            { typeof(Vector2), message => message.GetVector2() },
            { typeof(Vector3), message => message.GetVector3() },
            { typeof(Quaternion), message => message.GetQuaternion() },
            { typeof(Vector4), message => message.GetVector4() },
            { typeof(Matrix), message => message.GetMatrix() }
        };

        public static void Add<T>(this Message message, T value)
            where T : IMemoryPackable<T>
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            message.Add(MemoryPackSerializer.Serialize(value));
        }

        public static T Get<T>(this Message message)
        {
            if (s_getValue.TryGetValue(typeof(T), out Func<Message, object>? getValue))
                return (T)getValue(message);

            return MemoryPackSerializer.Deserialize<T>(message.GetBytes())
                ?? throw new Exception($"Deserializer for type {typeof(T)} not found.");
        }

        public static Message Add(this Message message, Vector2 value)
        {
            message.Add(value.X).Add(value.Y);
            return message;
        }

        public static Message Add(this Message message, Vector3 value)
        {
            message.Add(value.X).Add(value.Y).Add(value.Z);
            return message;
        }

        public static Message Add(this Message message, Quaternion value)
        {
            message.Add(value.X).Add(value.Y).Add(value.Z).Add(value.W);
            return message;
        }

        public static Message Add(this Message message, Vector4 value)
        {
            message.Add(value.X).Add(value.Y).Add(value.Z).Add(value.W);
            return message;
        }

        public static void Add(this Message message, Matrix matrix)
        {
            message.Add(matrix.Row1).Add(matrix.Row2)
                .Add(matrix.Row3).Add(matrix.Row4);
        }

        public static Vector2 GetVector2(this Message message)
        {
            return new Vector2(message.GetFloat(), message.GetFloat());
        }
        public static Vector3 GetVector3(this Message message)
        {
            return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
        }
        public static Quaternion GetQuaternion(this Message message)
        {
            return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        public static Vector4 GetVector4(this Message message)
        {
            return new Vector4
            {
                X = message.GetFloat(),
                Y = message.GetFloat(),
                Z = message.GetFloat(),
                W = message.GetFloat()
            };
        }

        public static Matrix GetMatrix(this Message message)
        {
            return new Matrix()
            {
                Row1 = message.GetVector4(),
                Row2 = message.GetVector4(),
                Row3 = message.GetVector4(),
                Row4 = message.GetVector4()
            };
        }
    }
}
