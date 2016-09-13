using System;

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
    }
}