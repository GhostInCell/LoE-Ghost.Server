using System;

namespace PNet
{
    public struct Serializer
    {
        public readonly Action<object, NetMessage> Serialize;
        public readonly Func<object, int> SizeOf;

        /// <summary>
        /// Functions to serialize and get size of the object. The object type is always going to be the provided type in the serializer dictionary
        /// </summary>
        /// <exception cref="ArgumentNullException">if either parameter is  null</exception>
        /// <param name="sizeOf">function that determines the size of the object</param>
        /// <param name="serialize">function that serializes the object</param>
        public Serializer(Func<object, int> sizeOf, Action<object, NetMessage> serialize)
        {
            SizeOf = sizeOf ?? throw new ArgumentNullException("sizeOf");
            Serialize = serialize ?? throw new ArgumentNullException("serialize");
        }
    }

    public struct Deserializer
    {
        public readonly Func<NetMessage, object> Deserialize;

        public Deserializer(Func<NetMessage, object> deserializer)
        {
            Deserialize = deserializer;
        }
    }
}
