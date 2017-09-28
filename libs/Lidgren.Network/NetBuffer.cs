using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lidgren.Network
{
    public partial class NetBuffer
	{
		/// <summary>
		/// Number of bytes to overallocate for each message to avoid resizing
		/// </summary>
		protected const int c_overAllocateAmount = 4;

		private static readonly Dictionary<Type, MethodInfo> s_readMethods;
		private static readonly Dictionary<Type, MethodInfo> s_writeMethods;

        protected internal byte[] m_data;
        protected internal int m_bitLength;
        protected internal int m_readPosition;

		/// <summary>
		/// Gets or sets the internal data buffer
		/// </summary>
		public byte[] Data
		{
			get { return m_data; }
			set { m_data = value; }
		}

		/// <summary>
		/// Gets or sets the length of the used portion of the buffer in bytes
		/// </summary>
		public int LengthBytes
		{
			get { return ((m_bitLength + 7) >> 3); }
			set
			{
				m_bitLength = value * 8;
				InternalEnsureBufferSize(m_bitLength);
			}
		}

		/// <summary>
		/// Gets or sets the length of the used portion of the buffer in bits
		/// </summary>
		public int LengthBits
		{
			get { return m_bitLength; }
			set
			{
				m_bitLength = value;
				InternalEnsureBufferSize(m_bitLength);
			}
		}

		/// <summary>
		/// Gets or sets the read position in the buffer, in bits (not bytes)
		/// </summary>
		public long Position
		{
			get { return (long)m_readPosition; }
			set { m_readPosition = (int)value; }
		}

		/// <summary>
		/// Gets the position in the buffer in bytes; note that the bits of the first returned byte may already have been read - check the Position property to make sure.
		/// </summary>
		public int PositionInBytes
		{
			get { return (int)(m_readPosition / 8); }
		}
		
        /// <summary>
        /// Gets the remaining number of bits in the buffer to be read
        /// </summary>
	    public long RemainingBits
	    {
            get { return (long)(m_bitLength - m_readPosition); }
	    }
        
        /// <summary>
        /// Clones this buffer into dest
        /// </summary>
        /// <param name="dest"></param>
        public void Clone(NetBuffer dest)
        {
            dest.m_data = new byte[m_data.Length];

            Buffer.BlockCopy(m_data, 0, dest.m_data, 0, m_data.Length);
            //foreach loops are faster in release builds until about 32k of bytes. supposedly. probably has something to do with the method call taking time.
            //dest.m_data = new byte[m_data.Length];
            //var i = 0;
            //foreach (var by in m_data)
            //{
            //    dest.m_data[i] = by;
            //    ++i;
            //}

            //set other values...
            dest.m_bitLength = m_bitLength;
            dest.m_readPosition = 0;
        }

		static NetBuffer()
		{
			s_readMethods = new Dictionary<Type, MethodInfo>();
			MethodInfo[] methods = typeof(NetIncomingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo mi in methods)
			{
				if (mi.GetParameters().Length == 0 && mi.Name.StartsWith("Read", StringComparison.InvariantCulture) && mi.Name.Substring(4) == mi.ReturnType.Name)
				{
					s_readMethods[mi.ReturnType] = mi;
				}
			}

			s_writeMethods = new Dictionary<Type, MethodInfo>();
			methods = typeof(NetOutgoingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo mi in methods)
			{
				if (mi.Name.Equals("Write", StringComparison.InvariantCulture))
				{
					ParameterInfo[] pis = mi.GetParameters();
					if (pis.Length == 1)
						s_writeMethods[pis[0].ParameterType] = mi;
				}
			}
		}
	}
}
