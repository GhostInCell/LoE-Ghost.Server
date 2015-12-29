using System.Collections.Generic;

namespace PNet
{
    /// <summary>
    /// A serializer for an array of INetSerializable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectArraySerializer<T> : INetSerializable
        where T : INetSerializable, new()
    {
        /// <summary>
        /// items
        /// </summary>
        public T[] Items;

        /// <summary>
        /// Whether or not the index is preserved with an array with nulls
        /// Takes an additional bit per index
        /// only makes sense for nullable types
        /// </summary>
        public bool PreserveIndex = false;

        // ReSharper disable StaticFieldInGenericType
        private static readonly bool IsValueType = typeof(T).IsValueType;
        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// Create a new serializer
        /// </summary>
        public ObjectArraySerializer()
        {
        }

        /// <summary>
        /// Create a new serializer from the specified array
        /// </summary>
        /// <param name="newItems"></param>
        public ObjectArraySerializer(T[] newItems)
        {
            Items = newItems;
        }

        /// <summary>
        /// Create a new serializer from the specified list
        /// </summary>
        /// <param name="newItems"></param>
        public ObjectArraySerializer(List<T> newItems)
        {
            Items = newItems.ToArray();
        }

        /// <summary>
        /// Size when writing to the stream
        /// </summary>
        public int AllocSize
        {
            get
            {
                if (Items != null && Items.Length >= 1)
                    return (4 + Items[0].AllocSize + (PreserveIndex ? 1 : 0)) * Items.Length;
                return 0;
            }
        }

        /// <summary>
        /// Deserialize the array from the message
        /// </summary>
        /// <param name="message"></param>
        public void OnDeserialize(NetMessage message)
        {
            var length = message.ReadInt32();
            Items = new T[length];

            for (int i = 0; i < length; i++)
            {
                var hasValue = true;
                if (PreserveIndex && !IsValueType)
                {
                    hasValue = message.ReadBoolean();
                }
                if (!hasValue) continue;

                var t = new T();
                t.OnDeserialize(message);
                Items[i] = t;
            }
        }

        /// <summary>
        /// Serialize the array to the message
        /// </summary>
        /// <param name="message"></param>
        public void OnSerialize(NetMessage message)
        {
            if (Items == null || Items.Length == 0)
            {
                message.Write(0);
                return;
            }

            message.Write(Items.Length);
            foreach (var item in Items)
            {
                if (!IsValueType)
                {
                    if (PreserveIndex)
                    {
                        message.Write(item != null);
                    }
                    if (item != null)
                        item.OnSerialize(message);
                }
                else
                {
                    item.OnSerialize(message);
                }
            }
        }
    }
}
