using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace PNet
{
    public class SerializationManager
    {
        private readonly Dictionary<Type, Serializer> _serializers = new Dictionary<Type, Serializer>();
        private readonly Dictionary<Type, Serializer> _internalSerializers = new Dictionary<Type, Serializer>();
        
        private readonly Dictionary<Type, Deserializer> _deserializers = new Dictionary<Type, Deserializer>();
        private readonly Dictionary<Type, Deserializer> _internalDeserializers = new Dictionary<Type, Deserializer>();

        public SerializationManager()
        {
            FillDefaultSerializer(_internalSerializers);
            FillDefaultDeserializer(_internalDeserializers);
        }

        public void Add(Type type, Serializer serializer, Deserializer deserializer, bool createArraySerialization = true)
        {
            _serializers[type] = serializer;
            _deserializers[type] = deserializer;
            if (createArraySerialization)
            {
                var svp = CreateArraySerializer(type, serializer);
                var dvp = CreateArrayDeserializer(type, deserializer);
                _serializers[svp.Key] = svp.Value;
                _deserializers[dvp.Key] = dvp.Value;
            }
        }

        public bool Remove(Type type, bool includeArraySerialization = true)
        {
            _serializers.Remove(type);
            var ret = _deserializers.Remove(type);
            if (includeArraySerialization)
            {
                var arrType = type.MakeArrayType();
                _serializers.Remove(arrType);
                _deserializers.Remove(arrType);
            }
            return ret;
        }

        /// <summary>
        /// Get the size of object o, via inetserializable, defined serializer, or built-in serializer (in that order)
        /// </summary>
        /// <exception cref="NotImplementedException">When serialization does not resolve</exception>
        /// <param name="o"></param>
        /// <returns></returns>
        public int SizeOf(object o)
        {
            var iser = o as INetSerializable;
            if (iser != null)
            {
                return iser.AllocSize;
            }

            var aType = o.GetType();
            Serializer ser;
            if (_serializers.TryGetValue(aType, out ser))
                return ser.SizeOf(o);
            if (_internalSerializers.TryGetValue(aType, out ser))
                return ser.SizeOf(o);
            if (aType.IsEnum)
            {
                var etype = Enum.GetUnderlyingType(aType);
                if (_internalSerializers.TryGetValue(etype, out ser))
                {
                    return ser.SizeOf(Convert.ChangeType(o, etype));
                }
            }
            var list = o as IEnumerable<INetSerializable>;
            if (list != null)
            {
                var size = 4;
                foreach (var item in list)
                    size += item?.AllocSize ?? 0;
                return size;
            }
            var arr = o as Array;
            if (arr != null)
            {
                var etype = aType.GetElementType();
                if (TypeIsINet(etype))
                {
                    var size = 0;
                    foreach (var aro in arr)
                    {
                        iser = aro as INetSerializable;
                        if (iser != null)
                            size += iser.AllocSize;
                    }
                    return size;
                }
            }
            throw new NotImplementedException("No size getter defined for " + aType);
        }

        /// <summary>
        /// serialize the object, via inetserializable, defined serializer, or built-in serializer (in that order)
        /// </summary>
        /// <exception cref="NotImplementedException">When serialization does not resolve</exception>
        /// <param name="o"></param>
        /// <param name="msg"></param>
        public void Serialize(object o, NetMessage msg)
        {
            var iser = o as INetSerializable;
            if (iser != null)
            {
                iser.OnSerialize(msg);
                return;
            }

            var aType = o.GetType();
            Serializer ser;
            if (_serializers.TryGetValue(aType, out ser))
            {
                ser.Serialize(o, msg);
                return;
            }

            if (_internalSerializers.TryGetValue(aType, out ser))
            {
                ser.Serialize(o, msg);
                return;
            }

            if (aType.IsEnum)
            {
                var etype = Enum.GetUnderlyingType(aType);
                if (_internalSerializers.TryGetValue(etype, out ser))
                {
                    ser.Serialize(Convert.ChangeType(o, etype), msg);
                    return;
                }
            }
            var list = o as IEnumerable<INetSerializable>;
            if (list != null)
            {
                msg.Write(list.Count());
                foreach (var item in list)
                    item.OnSerialize(msg);
                return;
            }
            var arr = o as Array;
            if (arr != null)
            {
                var etype = aType.GetElementType();
                if (TypeIsINet(etype))
                {
                    SerializeINetArray(arr, msg);
                    return;
                }
            }
            throw new NotImplementedException("No serializer defined for " + aType);
        }

        private static void SerializeINetArray(Array array, NetMessage msg)
        {
            var count = array.Length;
            msg.Write(count);
            foreach (var aro in array)
            {
                var iser = aro as INetSerializable;
                if (iser != null)
                {
                    iser.OnSerialize(msg);
                }
            }
        }

        /// <summary>
        /// whether or not the type has defined serialization
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CanSerialize(Type type)
        {
            if (_serializers.ContainsKey(type))
                return true;
            if (_internalSerializers.ContainsKey(type))
                return true;
            if (TypeIsINet(type))
            {
                return true;
            }
            if (type.IsEnum)
            {
                var etype = Enum.GetUnderlyingType(type);
                if (_internalSerializers.ContainsKey(etype))
                {
                    return true;
                }
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && TypeIsINet(type.GenericTypeArguments[0]))
                return true;
            if (type.IsArray)
            {
                var etype = type.GetElementType();
                if (TypeIsINet(etype))
                {
                    return true;
                }
            }
            return false;
        }


        static readonly ConcurrentDictionary<Type, bool> TypesAreINet = new ConcurrentDictionary<Type, bool>();

        bool TypeIsINet(Type type)
        {
            bool value;
            if (TypesAreINet.TryGetValue(type, out value))
                return value;
            
            var isInet = type.GetInterfaces().Contains(typeof(INetSerializable));
            TypesAreINet[type] = isInet;
            return isInet;
        }

        public T Deserialize<T>(NetMessage message)
        {
            return (T) Deserialize(typeof (T), message);
        }

        public object Deserialize(Type type, NetMessage message)
        {
            if (TypeIsINet(type))
            {
                var deser = (GetNew(type) as INetSerializable);
                if (deser != null)
                {
                    deser.OnDeserialize(message);
                    return deser;
                }
            }

            Deserializer deserializer;
            if (_deserializers.TryGetValue(type, out deserializer))
            {
                return deserializer.Deserialize(message);
            }

            if (_internalDeserializers.TryGetValue(type, out deserializer))
            {
                return deserializer.Deserialize(message);
            }

            if (type.IsEnum)
            {
                var etype = Enum.GetUnderlyingType(type);
                if (_internalDeserializers.TryGetValue(etype, out deserializer))
                {
                    return Enum.ToObject(type, deserializer.Deserialize(message));
                }
            }
            if (type.IsArray)
            {
                var etype = type.GetElementType();
                if (TypeIsINet(etype))
                {
                    return DeserializeArrayInet(etype, message);
                }
            }

            return null;
        }

        private static object DeserializeArrayInet(Type etype, NetMessage message)
        {
            var count = message.ReadInt32();
            var array = Array.CreateInstance(etype, count);
            for (int i = 0; i < count; i++)
            {
                var deser = (GetNew(etype) as INetSerializable);
                if (deser != null)
                {
                    deser.OnDeserialize(message);
                }
                array.SetValue(deser, i);
            }
            return array;
        }

        internal static object GetNew(Type type)
        {
            Func<object> del;
            if (Ctors.TryGetValue(type, out del))
                return del();

            if (type.HasDefaultConstructor())
            {
                Expression @new = Expression.New(type);
                if (type.IsValueType) //lambda won't box by default, which is required for value types.
                    @new = Expression.Convert(@new, typeof (object));

                del = (Func<object>)Expression.Lambda(typeof(Func<object>), @new).Compile();
            }
            else
            {
                del = () => null;
            }
            
            Ctors[type] = del;
            return del();
        }

        static readonly ConcurrentDictionary<Type, Func<object>> Ctors = new ConcurrentDictionary<Type, Func<object>>();

        public bool CanDeserialize(Type type)
        {
            if (_deserializers.ContainsKey(type))
                return true;
            if (_internalDeserializers.ContainsKey(type))
                return true;
            if (TypeIsINet(type) && type.HasDefaultConstructor())
            {
                return true;
            }
            if (type.IsEnum)
            {
                var etype = Enum.GetUnderlyingType(type);
                if (_internalDeserializers.ContainsKey(etype))
                {
                    return true;
                }
            }
            if (type.IsArray)
            {
                var etype = type.GetElementType();
                if (TypeIsINet(etype) && etype.HasDefaultConstructor())
                    return true;
            }
            return false;
        }

        static void FillDefaultSerializer(Dictionary<Type, Serializer> serializers)
        {
            serializers[typeof(byte)] = new Serializer(o => sizeof(byte),
                (o, message) => message.Write((byte)o));
            serializers[typeof(sbyte)] = new Serializer(o => sizeof(sbyte),
                (o, message) => message.Write((sbyte)o));
            serializers[typeof(short)] = new Serializer(o => sizeof(short),
                (o, message) => message.Write((short)o));
            serializers[typeof(ushort)] = new Serializer(o => sizeof(ushort),
                (o, message) => message.Write((ushort)o));
            serializers[typeof(int)] = new Serializer(o => sizeof(int),
                (o, message) => message.Write((int)o));
            serializers[typeof(uint)] = new Serializer(o => sizeof(uint),
                (o, message) => message.Write((uint)o));
            serializers[typeof(long)] = new Serializer(o => sizeof(long),
                (o, message) => message.Write((long)o));
            serializers[typeof(ulong)] = new Serializer(o => sizeof(ulong),
                (o, message) => message.Write((ulong)o));
            serializers[typeof(float)] = new Serializer(o => sizeof(float),
                (o, message) => message.Write((float)o));
            serializers[typeof(double)] = new Serializer(o => sizeof(double),
                (o, message) => message.Write((double)o));
            serializers[typeof(bool)] = new Serializer(o => sizeof(bool),
                (o, message) => message.Write((bool)o));
            serializers[typeof(string)] = new Serializer(o => ((string)o).Length * 2 + 8,
                (o, message) => message.Write((string)o));
            serializers[typeof(IPEndPoint)] = new Serializer(o =>
            {
                //lidgren serializes the byte length as a byte, + 2 for for ushort port number.
                var ip = (IPEndPoint) o;
                var bytes = ip.Address.GetAddressBytes();
                return 3 + bytes.Length;
            }, (o, message) => message.Write((IPEndPoint)o));
            serializers[typeof(Guid)] = new Serializer(o => 16, (o, message) => message.Write((Guid)o));
            serializers[typeof(DateTime)] = new Serializer(o => 8, (o, message) => message.Write((DateTime)o));

            //arrays
            serializers[typeof(byte[])] = new Serializer(
                o => ((byte[])o).Length + 4,
                (o, message) =>
                {
                    var bytes = (byte[])o;
                    message.Write(bytes.Length);
                    message.Write(bytes);
                });

            serializers[typeof(int[])] = new Serializer(
                o => ((int[])o).Length * sizeof(int) + 4,
                (o, message) =>
                {
                    var arr = (int[])o;
                    byte[] result = new byte[arr.Length * sizeof(int)];
                    Buffer.BlockCopy(arr, 0, result, 0, result.Length);
                    message.Write(arr.Length);
                    message.Write(result);
                });

            //set up array serializers
            foreach (var ser in serializers.ToArray())
            {
                if (ser.Key.IsArray) continue;
                var nvp = CreateArraySerializer(ser.Key, ser.Value);
                serializers[nvp.Key] = nvp.Value;
            }
        }

        private static void FillDefaultDeserializer(Dictionary<Type, Deserializer> deserializers)
        {
            deserializers[typeof(byte)] = new Deserializer(message => message.ReadByte());
            deserializers[typeof(sbyte)] = new Deserializer(message => message.ReadSByte());
            deserializers[typeof(short)] = new Deserializer(message => message.ReadInt16());
            deserializers[typeof(ushort)] = new Deserializer(message => message.ReadUInt16());
            deserializers[typeof(int)] = new Deserializer(message => message.ReadInt32());
            deserializers[typeof(uint)] = new Deserializer(message => message.ReadUInt32());
            deserializers[typeof(long)] = new Deserializer(message => message.ReadInt64());
            deserializers[typeof(ulong)] = new Deserializer(message => message.ReadUInt64());
            deserializers[typeof(float)] = new Deserializer(message => message.ReadSingle());
            deserializers[typeof(double)] = new Deserializer(message => message.ReadDouble());
            deserializers[typeof(bool)] = new Deserializer(message => message.ReadBoolean());
            deserializers[typeof(string)] = new Deserializer(message => message.ReadString());
            deserializers[typeof(IPEndPoint)] = new Deserializer(message => message.ReadIPEndPoint());
            deserializers[typeof(Guid)] = new Deserializer(message => message.ReadGuid());
            deserializers[typeof(DateTime)] = new Deserializer(message => message.ReadDateTime());

            //arrays
            deserializers[typeof(byte[])] = new Deserializer(message => message.ReadBytes(message.ReadInt32()));
            deserializers[typeof(int[])] = new Deserializer(message =>
            {
                var length = message.ReadInt32();
                var arr = new int[length];
                var bytes = message.ReadBytes(length*sizeof (int));
                Buffer.BlockCopy(bytes, 0, arr, 0, bytes.Length);
                return arr;
            });

            //set up array serializers
            foreach (var des in deserializers.ToArray())
            {
                if (des.Key.IsArray) continue;
                var nvp = CreateArrayDeserializer(des.Key, des.Value);
                deserializers[nvp.Key] = nvp.Value;
            }
        }

        static KeyValuePair<Type, Serializer> CreateArraySerializer(Type type, Serializer serializer)
        {
            var arrType = type.MakeArrayType();
            var arrSer = new Serializer(o =>
            {
                var arr = (Array)o;
                int size = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    size += serializer.SizeOf(arr.GetValue(i));
                }
                return size + 4;
            },
            (o, message) =>
            {
                var arr = (Array)o;
                message.Write(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    serializer.Serialize(arr.GetValue(i), message);
                }
            });

            return new KeyValuePair<Type, Serializer>(arrType, arrSer);
        }

        static KeyValuePair<Type, Deserializer> CreateArrayDeserializer(Type type, Deserializer deserializer)
        {
            var arrType = type.MakeArrayType();
            var arrDeser = new Deserializer(message =>
            {
                var size = message.ReadInt32();
                var arr = Array.CreateInstance(type, size);
                for (int i = 0; i < arr.Length; i++)
                {
                    arr.SetValue(deserializer.Deserialize(message), i);
                }
                return arr;
            });


            return new KeyValuePair<Type, Deserializer>(arrType, arrDeser);
        }
    }
}
