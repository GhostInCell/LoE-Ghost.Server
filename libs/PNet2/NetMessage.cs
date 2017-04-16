using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
#if LIDGREN
using Lidgren.Network;
#endif

namespace PNet
{
    public class NetMessage : NetBuffer
    {
#if LIDGREN
        internal NetConnection Sender;

        double GetLocalTime(double value)
        {
            return Sender.GetLocalTime(value);
        }
#elif UDPKIT
        double GetLocalTime(double value)
        {
            throw new NotImplementedException();
        }

        private double ReadTime(object sender, bool highPrecision)
        {
            throw new NotImplementedException();
        }

        internal object Sender;

        internal ReliabilityMode Reliability;
#else
        internal IPEndPoint Sender;

        private double ReadTime(IPEndPoint sender, bool highPrecision)
        {
            throw new NotImplementedException();
        }

        double GetLocalTime(double value)
        {
            throw new NotImplementedException();
        }
#endif

        //todo: implement our own stuff instead of use lidgren

        /// <summary>
        /// attempt to read a guid from the message. 16 bytes.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool ReadGuid(out Guid guid)
        {
            byte[] bytes;
            if (!ReadBytes(16, out bytes))
            {
                guid = default(Guid);
                return false;
            }
            guid = new Guid(bytes);
            return true;
        }
        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        /// <summary>
        /// Write a guid to the message. 16 bytes.
        /// </summary>
        /// <param name="guid"></param>
        public void Write(Guid guid)
        {
            Write(guid.ToByteArray());
        }

        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary(ReadInt64());
        }

        /// <summary>
        /// Attempt to read a datetime from the message. 8 bytes.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool ReadDateTime(out DateTime time)
        {
            if (RemainingBits < 64)
            {
                time = default(DateTime);
                return false;
            }
            time = ReadDateTime();
            return true;
        }

        /// <summary>
        /// Write a date time. 8 bytes.
        /// </summary>
        /// <param name="time"></param>
        public void Write(DateTime time)
        {
            Write(time.ToBinary());
        }

        /// <summary>
        /// Read the time as sent by the sender
        /// </summary>
        /// <param name="highPrecision"></param>
        /// <returns></returns>
        public double ReadTime(bool highPrecision)
        {
            if (Sender == null)
                return 0;
            return ReadTime(Sender, highPrecision);
        }

        /// <summary>
        /// Write Utilities.Now to a fixed point.
        /// max time is 213503982.33460129185 or 49.7102696 days, depending on highPrecision (to 6 or 3 decimal places/second)
        /// </summary>
        /// <param name="highPrecision"></param>
        public void WriteFixedTime(bool highPrecision)
        {
            WriteFixedTime(Utilities.Now, highPrecision);
        }

        /// <summary>
        /// write the time to a fixed point. Only works for positive values.
        /// max value is ulong/1000000 or uint/1000, depending on highPrecision
        /// </summary>
        /// <param name="time"></param>
        /// <param name="highPrecision"></param>
        public void WriteFixedTime(double time, bool highPrecision)
        {
            if (highPrecision)
            {
                Write((ulong)(time * 1000000));
            }
            else
            {
                Write((uint)(time * 1000));
            }
        }

        /// <summary>
        /// read a fixed-point time.
        /// </summary>
        /// <param name="highPrecision"></param>
        /// <returns></returns>
        public double ReadFixedTime(bool highPrecision)
        {
            double value;
            if (highPrecision)
            {
                value = ReadUInt64() / 1000000d;
            }
            else
            {
                value = ReadUInt32() / 1000d;
            }

            return GetLocalTime(value);
        }

        /// <summary>
        /// write the size of the message to the beginning 2 bytes. Maximum of 65535 length supported.
        /// </summary>
        internal void WriteSize()
        {
            var size = LengthBytes - 2;
            NetBitWriter.WriteUInt16(checked((ushort)size), 16, Data, 0);
        }

        #region Message recycling

        void RecycleReset()
        {
            Position = 0;
            LengthBits = 0;
            Data = null;
        }

        private const int MaxRecycleSize = SizeMaxRecycle * 32;
        private const int SizeMaxRecycle = 50;
        
        private static readonly Stack<NetMessage> RecycleStack = new Stack<NetMessage>(MaxRecycleSize);
        private static readonly Stack<byte[]>[] ByteRecycle;

        static NetMessage()
        {
            var recycle = new Stack<byte[]>[32];
            //0 will always be skipped due to a minimum of 4 bytes in length due to the padding
            for (int i = 0; i < recycle.Length; i++)
            {
                recycle[i] = new Stack<byte[]>(SizeMaxRecycle);
            }
            ByteRecycle = recycle;
        }

        /// <summary>
        /// Get a message from the recycle for the specified size, with padding at the beginning for writing the length in
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        internal static NetMessage GetMessageSizePad(int size)
        {
            var msg = GetMessage(size + 2);
            msg.WritePadBits(16);
            return msg;
        }

        internal static NetMessage GetMessageSizePad(int size, ReliabilityMode rel, BroadcastMode broad, MsgType mType,
            SubMsgType sub = SubMsgType.Out)
        {
            var msg = GetMessage(size + 3);
            msg.WritePadBits(16);
            msg.Write(RpcUtils.GetHeader(rel, broad, mType, sub));
            return msg;
        }

        internal static NetMessage GetMessage(int size, ReliabilityMode rel, BroadcastMode broad, MsgType mType,
            SubMsgType sub = SubMsgType.Out)
        {
            var msg = GetMessage(size + 1);
            msg.Write(RpcUtils.GetHeader(rel, broad, mType, sub));
            return msg;
        }

        /// <summary>
        /// Get a message from the recycle (or new if recycle is empty), with the specified size as a starting size.
        /// Warning: do not get and recycle the same message on different threads, as this will cause issues and large garbage collection.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        internal static NetMessage GetMessage(int size)
        {
            if (size < 0)
                throw new ArgumentException("cannot get a message of negative size", "size");

            NetMessage ret = null;
            lock(RecycleStack)
                if (RecycleStack.Count > 0)
                    ret = RecycleStack.Pop();
            if (ret == null)
                ret = new NetMessage();

            //a little bit larger
            size = Math.Max(2, size);
            //just increase size to the next power of two, and return that
            size--;
            size |= size >> 1;   // Divide by 2^k for consecutive doublings of k up to 32,
            size |= size >> 2;   // and then or the results.
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            size++;

            var n = log2(size);

            //try and pop a byte array to reuse
            byte[] data = null;
            var stack = ByteRecycle[n];
            lock (stack)
                if (stack.Count > 0)
                {
                    data = stack.Pop();
                }

            ret.Data = data ?? new byte[size];

            return ret;
        }

        static int log2(int n)
        {
            int targetLevel = 0;
            while ((n >>= 1) != 0) ++targetLevel;
            return targetLevel;
        }

        /// <summary>
        /// Recycle the message. Thread safe.
        /// </summary>
        /// <param name="message"></param>
        public static void RecycleMessage(NetMessage message)
        {
            var data = message.Data;
            if (data == null) return;
            if (!IsPowerOfTwo(data.Length)) return;

            var n = log2(data.Length);
            var stack = ByteRecycle[n];
            lock (stack)
                if (stack.Count < SizeMaxRecycle)
                    stack.Push(data);

            message.RecycleReset();
            lock(RecycleStack)
            if (RecycleStack.Count < MaxRecycleSize)
            {
                RecycleStack.Push(message);
            }
        }

        private static bool IsPowerOfTwo(int x)
        {
            return ((x != 0) && (x & (x - 1)) == 0);
        }


        /// <summary>
        /// maximum size a given message can be
        /// </summary>
        public const ushort MaxMessageSize = ushort.MaxValue;

        /// <summary>
        /// read ushort length prefixed messages from the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readBytes"></param>
        /// <param name="bytesReceived"></param>
        /// <param name="nextMessage"></param>
        /// <param name="lengthBuffer"></param>
        /// <param name="bufferSize"></param>
        /// <param name="msgAction"></param>
        /// <returns>number of messages read from the buffer</returns>
        public static int GetMessages(byte[] buffer, int readBytes,
            ref int bytesReceived, ref NetMessage nextMessage, ref byte[] lengthBuffer, ref int bufferSize,
            Action<NetMessage> msgAction)
        {
            var i = 0;
            var messagesRead = 0;
            while (i < readBytes)
            {
                // Determine how many bytes we want to transfer to the buffer and transfer them
                var bytesAvailable = readBytes - i;
                if (nextMessage != null)
                {
                    // We're reading into the data buffer
                    var bytesRequested = readBytes - bytesReceived;

                    // Copy the incoming bytes into the buffer
                    var bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    nextMessage.Write(buffer, i, bytesTransferred);
                    i += bytesTransferred;

                    bytesReceived += bytesTransferred;
                    if (bytesReceived < bufferSize)
                    {
                        // We haven't gotten all the data buffer yet: just wait for more data to arrive
                    }
                    else
                    {
                        // We've gotten an entire packet
                        msgAction(nextMessage);
                        messagesRead++;

                        // Start reading the length buffer again
                        nextMessage = null;
                        bytesReceived = 0;
                        bufferSize = 0;
                    }
                }
                else
                {
                    // We're reading into the length prefix buffer
                    var bytesRequested = lengthBuffer.Length - bytesReceived;

                    // Copy the incoming bytes into the buffer
                    var bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Array.Copy(buffer, i, lengthBuffer, bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    bytesReceived += bytesTransferred;
                    if (bytesReceived != sizeof(ushort))
                    {
                        // We haven't gotten all the length buffer yet: just wait for more data to arrive
                    }
                    else
                    {
                        // We've gotten the length buffer
                        var length = BitConverter.ToUInt16(lengthBuffer, 0);

                        // Another sanity check is needed here for very large packets, to prevent denial-of-service attacks
                        if (length > MaxMessageSize)
                            throw new ProtocolViolationException("Message length " +
                                                                 length.ToString(
                                                                     System.Globalization.CultureInfo.InvariantCulture) +
                                                                 " is larger than maximum message size " +
                                                                 MaxMessageSize.ToString(
                                                                     System.Globalization.CultureInfo.InvariantCulture));

                        // Zero-length packets are allowed as keepalives
                        if (length == 0)
                        {
                            bytesReceived = 0;
                        }
                        else
                        {
                            // Create the data buffer and start reading into it
                            nextMessage = GetMessage(length);
                            bufferSize = length;
                            bytesReceived = 0;
                        }
                    }
                }
            }
            return messagesRead;
        }

        #endregion
    }
}
