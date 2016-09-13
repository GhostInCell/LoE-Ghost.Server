using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Numerics;
#if LIDGREN
using Lidgren.Network;
#endif

namespace PNet
{
    public static class Utilities
    {
        public static bool TryParseGuid(string str, out Guid guid)
        {
            if (str.Length != 32)
            {
                guid = new Guid();
                return false;
            }

            if (!Regex.IsMatch(str, "^[A-Fa-f0-9]{32}$|"))
            {
                guid = new Guid();
                return false;
            }

            try
            {
                guid = new Guid(str);
                return true;
            }
            catch (Exception)
            {
                guid = new Guid();
                return false;
            }
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

#if LIDGREN
        /// <summary>
        /// the current network time
        /// </summary>
        public static double Now { get { return NetTime.Now; } }
#else
        public static double Now { get { throw new NotImplementedException(); } }
#endif
    }

    public static class AttributeExtensions
    {
        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Private helper for searching attributes.</summary>
        /// <param name="type">The type which is searched for the attribute.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attribute. Interfaces will be searched, too.</param>
        /// <returns>An array that contains all the custom attributes, or an array with zero elements if no attributes are defined.</returns>
        private static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            if (!inherit)
            {
                return type.GetCustomAttributes(attributeType, false);
            }

            var attributeCollection = new Collection<object>();
            var baseType = type;

            do
            {
                baseType.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);
                baseType = baseType.BaseType;
            }
            while (baseType != null);

            foreach (var interfaceType in type.GetInterfaces())
            {
                GetCustomAttributes(interfaceType, attributeType, true).Apply(attributeCollection.Add);
            }

            var attributeArray = new object[attributeCollection.Count];
            attributeCollection.CopyTo(attributeArray, 0);
            return attributeArray;
        }

        /// <summary>Applies a function to every element of the list.</summary>
        private static void Apply<T>(this IEnumerable<T> enumerable, Action<T> function)
        {
            foreach (var item in enumerable)
            {
                function.Invoke(item);
            }
        }
    }

    public static class NetConverter
    {
        public struct Int16Serializer : INetSerializable
        {
            public static readonly Int16Serializer Zero = new Int16Serializer(0);

            public short Value;

            public int AllocSize
            {
                get
                {
                    return sizeof(short);
                }
            }

            public Int16Serializer(short value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadInt16();
            }

            public static implicit operator short(Int16Serializer value)
            {
                return value.Value;
            }
            public static implicit operator Int16Serializer(short value)
            {
                return new Int16Serializer(value);
            }
        }
        public struct Int32Serializer : INetSerializable
        {
            public static readonly Int32Serializer Zero = new Int32Serializer(0);

            public int Value;

            public int AllocSize
            {
                get
                {
                    return sizeof(int);
                }
            }

            public Int32Serializer(int value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadInt32();
            }

            public static implicit operator int(Int32Serializer value)
            {
                return value.Value;
            }
            public static implicit operator Int32Serializer(int value)
            {
                return new Int32Serializer(value);
            }
        }
        public struct Int64Serializer : INetSerializable
        {
            public static readonly Int64Serializer Zero = new Int64Serializer(0);

            public long Value;

            public int AllocSize
            {
                get
                {
                    return sizeof(long);
                }
            }

            public Int64Serializer(long value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadInt64();
            }

            public static implicit operator long(Int64Serializer value)
            {
                return value.Value;
            }
            public static implicit operator Int64Serializer(long value)
            {
                return new Int64Serializer(value);
            }
        }
        public struct UInt16Serializer : INetSerializable
        {
            public static readonly UInt16Serializer Zero = new UInt16Serializer(0);

            public ushort Value;

            public int AllocSize
            {
                get
                {
                    return sizeof(ushort);
                }
            }

            public UInt16Serializer(ushort value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadUInt16();
            }

            public static implicit operator ushort(UInt16Serializer value)
            {
                return value.Value;
            }
            public static implicit operator UInt16Serializer(ushort value)
            {
                return new UInt16Serializer(value);
            }
        }
        public struct StringSerializer : INetSerializable
        {
            public static readonly StringSerializer Empty = new StringSerializer(string.Empty);

            public string Value;

            public int AllocSize
            {
                get
                {
                    return Value?.Length * 2 ?? 0 + 4;
                }
            }

            public StringSerializer(string value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadString();
            }

            public static implicit operator string(StringSerializer value)
            {
                return value.Value;
            }
            public static implicit operator StringSerializer(string value)
            {
                return new StringSerializer(value);
            }
        }
        public struct BooleanSerializer : INetSerializable
        {
            public bool Value;

            public int AllocSize
            {
                get
                {
                    return 1;
                }
            }

            public BooleanSerializer(bool value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value = message.ReadBoolean();
            }

            public static implicit operator bool(BooleanSerializer value)
            {
                return value.Value;
            }
            public static implicit operator BooleanSerializer(bool value)
            {
                return new BooleanSerializer(value);
            }
        }
        public struct Vector3Serializer : INetSerializable
        {
            public Vector3 Value;

            public int AllocSize
            {
                get
                {
                    return sizeof(float) * 3;
                }
            }

            public Vector3Serializer(Vector3 value)
            {
                Value = value;
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write(Value.X);
                message.Write(Value.Y);
                message.Write(Value.Z);
            }

            public void OnDeserialize(NetMessage message)
            {
                Value.X = message.ReadSingle();
                Value.Y = message.ReadSingle();
                Value.Z = message.ReadSingle();
            }

            public static implicit operator Vector3(Vector3Serializer value)
            {
                return value.Value;
            }
            public static implicit operator Vector3Serializer(Vector3 value)
            {
                return new Vector3Serializer(value);
            }
        }
    }
}
