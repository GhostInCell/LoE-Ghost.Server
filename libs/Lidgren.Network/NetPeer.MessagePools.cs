using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Network
{
	public partial class NetPeer
	{
        private Stack<byte[]>[] _byteRecycle;

		internal List<byte[]> m_storagePool;
		private NetQueue<NetOutgoingMessage> m_outgoingMessagesPool;
		private NetQueue<NetIncomingMessage> m_incomingMessagesPool;

		internal int m_storagePoolBytes;
		internal int m_storageSlotsUsedCount;
		private int m_maxCacheCount;

		private void InitializePools()
		{
			m_storageSlotsUsedCount = 0;

			if (m_configuration.UseMessageRecycling)
			{
				m_storagePool = new List<byte[]>(16);
				m_outgoingMessagesPool = new NetQueue<NetOutgoingMessage>(4);
				m_incomingMessagesPool = new NetQueue<NetIncomingMessage>(4);
			}
			else
			{
				m_storagePool = null;
				m_outgoingMessagesPool = null;
				m_incomingMessagesPool = null;
			}

			m_maxCacheCount = m_configuration.RecycledCacheMaxCount;

            var recycle = new Stack<byte[]>[32];
            //technically, 0 will always be skipped, but nulls are bad.
            for (int i = 0; i < recycle.Length; i++)
            {
                recycle[i] = new Stack<byte[]>(m_maxCacheCount);
            }
            _byteRecycle = recycle;
		}

		internal byte[] GetStorage(int minimumCapacityInBytes)
		{
			if (m_storagePool == null)
				return new byte[minimumCapacityInBytes];

            //no less than two bytes
            var size = Math.Max(2, minimumCapacityInBytes);
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
            var stack = _byteRecycle[n];
            lock (stack)
                if (stack.Count > 0)
                {
                    data = stack.Pop();
                }

		    if (data != null)
		    {
		        return data;
		    }
		    m_statistics.m_bytesAllocated += size;
            return new byte[size];

			lock (m_storagePool)
			{
				for (int i = 0; i < m_storagePool.Count; i++)
				{
					byte[] retval = m_storagePool[i];
					if (retval != null && retval.Length >= minimumCapacityInBytes)
					{
						m_storagePool[i] = null;
						m_storageSlotsUsedCount--;
						m_storagePoolBytes -= retval.Length;
						return retval;
					}
				}
			}
			m_statistics.m_bytesAllocated += minimumCapacityInBytes;
			return new byte[minimumCapacityInBytes];
		}

        private static int log2(int n)
        {
            int targetLevel = 0;
            while ((n >>= 1) != 0) ++targetLevel;
            return targetLevel;
        }
        private static bool IsPowerOfTwo(int x)
        {
            return ((x != 0) && (x & (x - 1)) == 0);
        }

		internal void Recycle(byte[] storage)
		{
			if (m_storagePool == null || storage == null)
				return;

            if (!IsPowerOfTwo(storage.Length)) return;

            var n = log2(storage.Length);
            var stack = _byteRecycle[n];
            lock (stack)
                if (stack.Count < m_maxCacheCount)
                    stack.Push(storage);
		    return;

			lock (m_storagePool)
			{
				int cnt = m_storagePool.Count;
				for (int i = 0; i < cnt; i++)
				{
					if (m_storagePool[i] == null)
					{
						m_storageSlotsUsedCount++;
						m_storagePoolBytes += storage.Length;
						m_storagePool[i] = storage;
						return;
					}
				}

				if (m_storagePool.Count >= m_maxCacheCount)
				{
					// pool is full; replace randomly chosen entry to keep size distribution
					var idx = NetRandom.Instance.Next(m_storagePool.Count);

					m_storagePoolBytes -= m_storagePool[idx].Length;
					m_storagePoolBytes += storage.Length;
					
					m_storagePool[idx] = storage; // replace
				}
				else
				{
					m_storageSlotsUsedCount++;
					m_storagePoolBytes += storage.Length;
					m_storagePool.Add(storage);
				}
			}
		}

		/// <summary>
		/// Creates a new message for sending
		/// </summary>
		public NetOutgoingMessage CreateMessage()
		{
			return CreateMessage(m_configuration.m_defaultOutgoingMessageCapacity);
		}

		/// <summary>
		/// Creates a new message for sending and writes the provided string to it
		/// </summary>
		public NetOutgoingMessage CreateMessage(string content)
		{
			var om = CreateMessage(2 + content.Length); // fair guess
			om.Write(content);
			return om;
		}

		/// <summary>
		/// Creates a new message for sending
		/// </summary>
		/// <param name="initialCapacity">initial capacity in bytes</param>
		public NetOutgoingMessage CreateMessage(int initialCapacity)
		{
			NetOutgoingMessage retval;
			if (m_outgoingMessagesPool == null || !m_outgoingMessagesPool.TryDequeue(out retval))
				retval = new NetOutgoingMessage();

			NetException.Assert(retval.m_recyclingCount == 0, "Wrong recycling count! Should be zero" + retval.m_recyclingCount);

			if (initialCapacity > 0)
				retval.m_data = GetStorage(initialCapacity);

			return retval;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, byte[] useStorageData)
		{
			NetIncomingMessage retval;
			if (m_incomingMessagesPool == null || !m_incomingMessagesPool.TryDequeue(out retval))
				retval = new NetIncomingMessage(tp);
			else
				retval.m_incomingMessageType = tp;
			retval.m_data = useStorageData;
			return retval;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, int minimumByteSize)
		{
			NetIncomingMessage retval;
			if (m_incomingMessagesPool == null || !m_incomingMessagesPool.TryDequeue(out retval))
				retval = new NetIncomingMessage(tp);
			else
				retval.m_incomingMessageType = tp;
			retval.m_data = GetStorage(minimumByteSize);
			return retval;
		}

		/// <summary>
		/// Recycles a NetIncomingMessage instance for reuse; taking pressure off the garbage collector
		/// </summary>
		public void Recycle(NetIncomingMessage msg)
		{
			if (m_incomingMessagesPool == null || msg == null)
				return;

			NetException.Assert(m_incomingMessagesPool.Contains(msg) == false, "Recyling already recycled incoming message! Thread race?");

			byte[] storage = msg.m_data;
			msg.m_data = null;
			Recycle(storage);
			msg.Reset();

			if (m_incomingMessagesPool.Count < m_maxCacheCount)
				m_incomingMessagesPool.Enqueue(msg);
		}

		/// <summary>
		/// Recycles a list of NetIncomingMessage instances for reuse; taking pressure off the garbage collector
		/// </summary>
		public void Recycle(IEnumerable<NetIncomingMessage> toRecycle)
		{
			if (m_incomingMessagesPool == null)
				return;
			foreach (var im in toRecycle)
				Recycle(im);
		}

		public void Recycle(NetOutgoingMessage msg)
		{
			if (m_outgoingMessagesPool == null)
				return;
#if DEBUG
			NetException.Assert(m_outgoingMessagesPool.Contains(msg) == false, "Recyling already recycled outgoing message! Thread race?");
			if (msg.m_recyclingCount != 0)
				LogWarning("Wrong recycling count! should be zero; found " + msg.m_recyclingCount);
#endif
			// setting m_recyclingCount to zero SHOULD be an unnecessary maneuver, if it's not zero something is wrong
			// however, in RELEASE, we'll just have to accept this and move on with life
			msg.m_recyclingCount = 0;

			byte[] storage = msg.m_data;
			msg.m_data = null;

			// message fragments cannot be recycled
			// TODO: find a way to recycle large message after all fragments has been acknowledged; or? possibly better just to garbage collect them
			if (msg.m_fragmentGroup == 0)
				Recycle(storage);

			msg.Reset();
			if (m_outgoingMessagesPool.Count < m_maxCacheCount)
				m_outgoingMessagesPool.Enqueue(msg);
		}

		/// <summary>
		/// Creates an incoming message with the required capacity for releasing to the application
		/// </summary>
		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, string text)
		{
			NetIncomingMessage retval;
			if (string.IsNullOrEmpty(text))
			{
				retval = CreateIncomingMessage(tp, 1);
				retval.Write(string.Empty);
				return retval;
			}

			int numBytes = System.Text.Encoding.UTF8.GetByteCount(text);
			retval = CreateIncomingMessage(tp, numBytes + (numBytes > 127 ? 2 : 1));
			retval.Write(text);

			return retval;
		}
	}
}
