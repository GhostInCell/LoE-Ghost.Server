﻿using System;

namespace PNet
{
    /// <summary>
    /// Interface used for objects that can read and write from network streams
    /// </summary>
    public interface INetSerializable
    {
        /// <summary>
        /// write to the message
        /// </summary>
        /// <param name="message">message to write to</param>
        void OnSerialize(NetMessage message);
        /// <summary>
        /// read the message
        /// </summary>
        /// <param name="message">message to read from</param>
        void OnDeserialize(NetMessage message);

        /// <summary>
        /// size to allocate for bytes in the message.  if you're under, it'll result in array resizing.
        /// </summary>
        int AllocSize { get; }
    }

    public static class INetSerializableExtensions
    {
        /// <summary>
        /// get the allocation size from the specified serializing objects
        /// </summary>
        /// <param name="prealloc"></param>
        /// <param name="towrite"></param>
        public static void AllocSize(this INetSerializable[] towrite, ref int prealloc)
        {
            foreach (var arg in towrite)
            {
                prealloc += arg.AllocSize;
            }
        }

        /// <summary>
        /// write all the serializing objects to the stream
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="towrite"></param>
        public static void WriteParams(ref NetMessage msg, INetSerializable[] towrite)
        {
            foreach (var arg in towrite)
            {
                arg.OnSerialize(msg);
            }
        }

        /// <summary>
        /// Serialize to an IntSerializer. This will have issues if the enum isn't an int type.
        /// </summary>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static IntSerializer Serialize(this Enum enumeration)
        {
            return new IntSerializer(Convert.ToInt32(enumeration));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSerialize"> </typeparam>
    /// <typeparam name="TValue"> </typeparam>
    public abstract class ASerializable<TSerialize, TValue> : INetSerializable where TSerialize : ASerializable<TSerialize, TValue>, new()
    {
        /// <summary>
        /// value of the class
        /// </summary>
        public TValue Value;

        /// <summary>
        /// 
        /// </summary>
        public ASerializable() { }

        /// <summary>
        /// Initialize the object from deserializing from the message
        /// </summary>
        /// <param name="toDeserialize"></param>
        public ASerializable(NetMessage toDeserialize)
        {
            OnDeserialize(toDeserialize);
        }

        /// <summary>
        /// method to run when serializing to the message
        /// </summary>
        /// <param name="message"></param>
        public abstract void OnSerialize(NetMessage message);

        /// <summary>
        /// deserialize from the message
        /// </summary>
        /// <param name="message"></param>
        public abstract void OnDeserialize(NetMessage message);

        /// <summary>
        /// size in bytes
        /// </summary>
        public abstract int AllocSize { get; }

        [ThreadStatic]
        private static TSerialize _instance;

        /// <summary>
        /// A static instance of the class. Thread safe.
        /// </summary>
        public static TSerialize Instance
        {
            get { return _instance ?? (_instance = new TSerialize()); }
        }
        /// <summary>
        /// update Value with the newValue
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns>this</returns>
        public ASerializable<TSerialize, TValue> Update(TValue newValue)
        {
            Value = newValue;
            return this;
        }

        /// <summary>
        /// deserialize Instance.Value from the message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Instance.Value</returns>
        public static TValue Deserialize(NetMessage msg)
        {
            Instance.OnDeserialize(msg);
            return Instance.Value;
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }
    }

    /// <summary>
    /// serializer for strings
    /// </summary>
    public class StringSerializer : ASerializable<StringSerializer, string>
    {
        public StringSerializer(string value)
        {
            Value = value;
        }
        public StringSerializer() { Value = ""; }
        /// <summary>
        /// serialize
        /// </summary>
        /// <param name="message"></param>
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }
        /// <summary>
        /// deserialize
        /// </summary>
        /// <param name="message"></param>
        public override void OnDeserialize(NetMessage message)
        {
            message.ReadString(out Value);
        }
        /// <summary>
        /// get the size of the string in bytes
        /// </summary>
        public override int AllocSize
        {
            get { return Value.Length * 2; }
        }
    }

    /// <summary>
    /// serializer for integers
    /// </summary>
    public class IntSerializer : ASerializable<IntSerializer, int>
    {
        public IntSerializer() { }
        public IntSerializer(int value)
        {
            Value = value;
        }

        /// <summary>
        /// serialize
        /// </summary>
        /// <param name="message"></param>
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        /// <summary>
        /// deserialize
        /// </summary>
        /// <param name="message"></param>
        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadInt32();
        }

        /// <summary>
        /// size of the integer in bytes
        /// </summary>
        public override int AllocSize
        {
            get { return 4; }
        }
    }

    /// <summary>
    /// serializer for floats
    /// </summary>
    public class FloatSerializer : ASerializable<FloatSerializer, float>
    {
        /// <summary>
        /// create a new serializer for a float
        /// </summary>
        /// <param name="value"></param>
        public FloatSerializer(float value) { Value = value; }

        /// <summary>
        /// Create a new serializer with a value of 0
        /// </summary>
        public FloatSerializer() { }

        /// <summary>
        /// serialize
        /// </summary>
        /// <param name="message"></param>
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        /// <summary>
        /// deserialize
        /// </summary>
        /// <param name="message"></param>
        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadFloat();
        }

        /// <summary>
        /// get the size of this serializable
        /// </summary>
        public override int AllocSize
        {
            get { return 4; }
        }
    }

    /// <summary>
    /// serializer for a double
    /// </summary>
    public class DoubleSerializer : ASerializable<DoubleSerializer, double>
    {
        public DoubleSerializer() { }
        public DoubleSerializer(double value)
        {
            Value = value;
        }
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadDouble();
        }

        public override int AllocSize
        {
            get { return sizeof(double); }
        }
    }

    /// <summary>
    /// serializer for a short
    /// </summary>
    public class ShortSerializer : ASerializable<ShortSerializer, short>
    {
        /// <summary>
        /// 
        /// </summary>
        public ShortSerializer() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public ShortSerializer(short value)
        {
            Value = value;
        }
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadInt16();
        }

        public override int AllocSize
        {
            get { return 2; }
        }
    }

    /// <summary>
    /// serializer for a ushort
    /// </summary>
    public class UShortSerializer : ASerializable<UShortSerializer, ushort>
    {
        /// <summary>
        /// 
        /// </summary>
        public UShortSerializer() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public UShortSerializer(ushort value)
        {
            Value = value;
        }
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadUInt16();
        }

        public override int AllocSize
        {
            get { return 2; }
        }
    }

    /// <summary>
    /// class to serialize single bytes
    /// </summary>
    public class ByteSerializer : ASerializable<ByteSerializer, byte>
    {
        /// <summary>
        /// 
        /// </summary>
        public ByteSerializer() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public ByteSerializer(byte value)
        {
            Value = value;
        }
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadByte();
        }

        public override int AllocSize
        {
            get { return 1; }
        }
    }

    /// <summary>
    /// class to serialize single booleans
    /// </summary>
    public class BoolSerializer : ASerializable<BoolSerializer, bool>
    {
        /// <summary>
        /// 
        /// </summary>
        public BoolSerializer() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BoolSerializer(bool value)
        {
            Value = value;
        }
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value);
        }

        public override void OnDeserialize(NetMessage message)
        {
            Value = message.ReadBoolean();
        }

        public override int AllocSize
        {
            get { return 1; }
        }
    }

    /// <summary>
    /// class to serialize byte arrays
    /// </summary>
    public class ByteArraySerializer : ASerializable<ByteArraySerializer, byte[]>
    {
        /// <summary>
        /// serialize to the stream
        /// </summary>
        /// <param name="message"></param>
        public override void OnSerialize(NetMessage message)
        {
            message.Write(Value.Length);
            message.Write(Value);
        }

        /// <summary>
        /// deserialize from the stream
        /// </summary>
        /// <param name="message"></param>
        public override void OnDeserialize(NetMessage message)
        {
            var size = message.ReadInt32();
            message.ReadBytes(size, out Value);
        }

        /// <summary />
        public override int AllocSize { get { return Value.Length + 4; } }
    }
}
