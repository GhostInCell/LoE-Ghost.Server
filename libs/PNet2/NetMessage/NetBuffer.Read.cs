#if !LIDGREN
using System;
using System.Net;

namespace PNet
{
	/// <summary>
	/// Base class for NetIncomingMessage and NetOutgoingMessage
	/// </summary>
	public partial class NetBuffer
	{
	    /// <summary>
		/// Reads a boolean value (stored as a single bit) written using Write(bool)
		/// </summary>
		public bool ReadBoolean()
		{
			byte retval = NetBitWriter.ReadByte(m_data, 1, m_readPosition);
			m_readPosition += 1;
			return (retval > 0);
		}
		
		/// <summary>
		/// Reads a byte
		/// </summary>
		public byte ReadByte()
		{
			byte retval = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return retval;
		}

		/// <summary>
		/// Reads a byte and returns true or false for success
		/// </summary>
		public bool ReadByte(out byte result)
		{
			if (m_bitLength - m_readPosition < 8)
			{
				result = 0;
				return false;
			}
			result = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return true;
		}

		/// <summary>
		/// Reads a signed byte
		/// </summary>
		[CLSCompliant(false)]
		public sbyte ReadSByte()
		{
			byte retval = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return (sbyte)retval;
		}

		/// <summary>
		/// Reads 1 to 8 bits into a byte
		/// </summary>
		public byte ReadByte(int numberOfBits)
		{
			byte retval = NetBitWriter.ReadByte(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			return retval;
		}

		/// <summary>
		/// Reads the specified number of bytes
		/// </summary>
		public byte[] ReadBytes(int numberOfBytes)
		{
			byte[] retval = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, retval, 0);
			m_readPosition += (8 * numberOfBytes);
			return retval;
		}

		/// <summary>
		/// Reads the specified number of bytes and returns true for success
		/// </summary>
		public bool ReadBytes(int numberOfBytes, out byte[] result)
		{
			if (m_bitLength - m_readPosition + 7 < (numberOfBytes * 8))
			{
				result = null;
				return false;
			}

			result = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, result, 0);
			m_readPosition += (8 * numberOfBytes);
			return true;
		}

		/// <summary>
		/// Reads the specified number of bytes into a preallocated array
		/// </summary>
		/// <param name="into">The destination array</param>
		/// <param name="offset">The offset where to start writing in the destination array</param>
		/// <param name="numberOfBytes">The number of bytes to read</param>
		public void ReadBytes(byte[] into, int offset, int numberOfBytes)
		{
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, into, offset);
			m_readPosition += (8 * numberOfBytes);
		}

		/// <summary>
		/// Reads the specified number of bits into a preallocated array
		/// </summary>
		/// <param name="into">The destination array</param>
		/// <param name="offset">The offset where to start writing in the destination array</param>
		/// <param name="numberOfBits">The number of bits to read</param>
		public void ReadBits(byte[] into, int offset, int numberOfBits)
		{
			int numberOfWholeBytes = numberOfBits / 8;
			int extraBits = numberOfBits - (numberOfWholeBytes * 8);

			NetBitWriter.ReadBytes(m_data, numberOfWholeBytes, m_readPosition, into, offset);
			m_readPosition += (8 * numberOfWholeBytes);

			if (extraBits > 0)
				into[offset + numberOfWholeBytes] = ReadByte(extraBits);
		}

		/// <summary>
		/// Reads a 16 bit signed integer written using Write(Int16)
		/// </summary>
		public Int16 ReadInt16()
		{
			uint retval = NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
			m_readPosition += 16;
			return (short)retval;
		}

		/// <summary>
		/// Reads a 16 bit unsigned integer written using Write(UInt16)
		/// </summary>
		[CLSCompliant(false)]
		public UInt16 ReadUInt16()
		{
			uint retval = NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
			m_readPosition += 16;
			return (ushort)retval;
		}

		/// <summary>
		/// Reads a 32 bit signed integer written using Write(Int32)
		/// </summary>
		public Int32 ReadInt32()
		{
			uint retval = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return (Int32)retval;
		}

		/// <summary>
		/// Reads a 32 bit signed integer written using Write(Int32)
		/// </summary>
		[CLSCompliant(false)]
		public bool ReadInt32(out Int32 result)
		{
			if (m_bitLength - m_readPosition < 32)
			{
				result = 0;
				return false;
			}

			result = (Int32)NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return true;
		}

		/// <summary>
		/// Reads a signed integer stored in 1 to 32 bits, written using Write(Int32, Int32)
		/// </summary>
		public Int32 ReadInt32(int numberOfBits)
		{
			uint retval = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;

			if (numberOfBits == 32)
				return (int)retval;

			int signBit = 1 << (numberOfBits - 1);
			if ((retval & signBit) == 0)
				return (int)retval; // positive

			// negative
			unchecked
			{
				uint mask = ((uint)-1) >> (33 - numberOfBits);
				uint tmp = (retval & mask) + 1;
				return -((int)tmp);
			}
		}

		/// <summary>
		/// Reads an 32 bit unsigned integer written using Write(UInt32)
		/// </summary>
		[CLSCompliant(false)]
		public UInt32 ReadUInt32()
		{
			uint retval = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return retval;
		}

		/// <summary>
		/// Reads an 32 bit unsigned integer written using Write(UInt32) and returns true for success
		/// </summary>
		[CLSCompliant(false)]
		public bool ReadUInt32(out UInt32 result)
		{
			if (m_bitLength - m_readPosition < 32)
			{
				result = 0;
				return false;
			}
			result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return true;
		}

		/// <summary>
		/// Reads an unsigned integer stored in 1 to 32 bits, written using Write(UInt32, Int32)
		/// </summary>
		[CLSCompliant(false)]
		public UInt32 ReadUInt32(int numberOfBits)
		{
			UInt32 retval = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			return retval;
		}

		/// <summary>
		/// Reads a 64 bit unsigned integer written using Write(UInt64)
		/// </summary>
		[CLSCompliant(false)]
		public UInt64 ReadUInt64()
		{
			ulong low = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			ulong high = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);

			ulong retval = low + (high << 32);

			m_readPosition += 32;
			return retval;
		}

		/// <summary>
		/// Reads a 64 bit signed integer written using Write(Int64)
		/// </summary>
		public Int64 ReadInt64()
		{
			unchecked
			{
				ulong retval = ReadUInt64();
				long longRetval = (long)retval;
				return longRetval;
			}
		}

		/// <summary>
		/// Reads an unsigned integer stored in 1 to 64 bits, written using Write(UInt64, Int32)
		/// </summary>
		[CLSCompliant(false)]
		public UInt64 ReadUInt64(int numberOfBits)
		{
			ulong retval;
			if (numberOfBits <= 32)
			{
				retval = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			}
			else
			{
				retval = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
				retval |= NetBitWriter.ReadUInt32(m_data, numberOfBits - 32, m_readPosition) << 32;
			}
			m_readPosition += numberOfBits;
			return retval;
		}

		/// <summary>
		/// Reads a signed integer stored in 1 to 64 bits, written using Write(Int64, Int32)
		/// </summary>
		public Int64 ReadInt64(int numberOfBits)
		{
			return (long)ReadUInt64(numberOfBits);
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using Write(Single)
		/// </summary>
		public float ReadFloat()
		{
			return ReadSingle();
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using Write(Single)
		/// </summary>
		public float ReadSingle()
		{
			if ((m_readPosition & 7) == 0) // read directly
			{
				float retval = BitConverter.ToSingle(m_data, m_readPosition >> 3);
				m_readPosition += 32;
				return retval;
			}

			byte[] bytes = ReadBytes(4);
			return BitConverter.ToSingle(bytes, 0);
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using Write(Single)
		/// </summary>
		public bool ReadSingle(out float result)
		{
			if (m_bitLength - m_readPosition < 32)
			{
				result = 0.0f;
				return false;
			}

			if ((m_readPosition & 7) == 0) // read directly
			{
				result = BitConverter.ToSingle(m_data, m_readPosition >> 3);
				m_readPosition += 32;
				return true;
			}

			byte[] bytes = ReadBytes(4);
			result = BitConverter.ToSingle(bytes, 0);
			return true;
		}

		/// <summary>
		/// Reads a 64 bit floating point value written using Write(Double)
		/// </summary>
		public double ReadDouble()
		{
			if ((m_readPosition & 7) == 0) // read directly
			{
				// read directly
				double retval = BitConverter.ToDouble(m_data, m_readPosition >> 3);
				m_readPosition += 64;
				return retval;
			}

			byte[] bytes = ReadBytes(8);
			return BitConverter.ToDouble(bytes, 0);
		}

		//
		// Variable bit count
		//

		/// <summary>
		/// Reads a variable sized UInt32 written using WriteVariableUInt32()
		/// </summary>
		[CLSCompliant(false)]
		public uint ReadVariableUInt32()
		{
			int num1 = 0;
			int num2 = 0;
			while (m_bitLength - m_readPosition >= 8)
			{
				byte num3 = ReadByte();
				num1 |= (num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
					return (uint)num1;
			}

			// ouch; failed to find enough bytes; malformed variable length number?
			return (uint)num1;
		}

		/// <summary>
		/// Reads a variable sized UInt32 written using WriteVariableUInt32() and returns true for success
		/// </summary>
		[CLSCompliant(false)]
		public bool ReadVariableUInt32(out uint result)
		{
			int num1 = 0;
			int num2 = 0;
			while (m_bitLength - m_readPosition >= 8)
			{
				byte num3;
				if (ReadByte(out num3) == false)
				{
					result = 0;
					return false;
				}
				num1 |= (num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					result = (uint)num1;
					return true;
				}
			}
			result = (uint)num1;
			return false;
		}

		/// <summary>
		/// Reads a variable sized Int32 written using WriteVariableInt32()
		/// </summary>
		public int ReadVariableInt32()
		{
			uint n = ReadVariableUInt32();
			return (int)(n >> 1) ^ -(int)(n & 1); // decode zigzag
		}

		/// <summary>
		/// Reads a variable sized Int64 written using WriteVariableInt64()
		/// </summary>
		public Int64 ReadVariableInt64()
		{
			UInt64 n = ReadVariableUInt64();
			return (Int64)(n >> 1) ^ -(long)(n & 1); // decode zigzag
		}

		/// <summary>
		/// Reads a variable sized UInt32 written using WriteVariableInt64()
		/// </summary>
		[CLSCompliant(false)]
		public UInt64 ReadVariableUInt64()
		{
			UInt64 num1 = 0;
			int num2 = 0;
			while (m_bitLength - m_readPosition >= 8)
			{
				//if (num2 == 0x23)
				//	throw new FormatException("Bad 7-bit encoded integer");

				byte num3 = ReadByte();
				num1 |= ((UInt64)num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
					return num1;
			}

			// ouch; failed to find enough bytes; malformed variable length number?
			return num1;
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using WriteSignedSingle()
		/// </summary>
		/// <param name="numberOfBits">The number of bits used when writing the value</param>
		/// <returns>A floating point value larger or equal to -1 and smaller or equal to 1</returns>
		public float ReadSignedSingle(int numberOfBits)
		{
			uint encodedVal = ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return ((encodedVal + 1) / (float)(maxVal + 1) - 0.5f) * 2.0f;
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using WriteUnitSingle()
		/// </summary>
		/// <param name="numberOfBits">The number of bits used when writing the value</param>
		/// <returns>A floating point value larger or equal to 0 and smaller or equal to 1</returns>
		public float ReadUnitSingle(int numberOfBits)
		{
			uint encodedVal = ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return (encodedVal + 1) / (float)(maxVal + 1);
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using WriteRangedSingle()
		/// </summary>
		/// <param name="min">The minimum value used when writing the value</param>
		/// <param name="max">The maximum value used when writing the value</param>
		/// <param name="numberOfBits">The number of bits used when writing the value</param>
		/// <returns>A floating point value larger or equal to MIN and smaller or equal to MAX</returns>
		public float ReadRangedSingle(float min, float max, int numberOfBits)
		{
			float range = max - min;
			int maxVal = (1 << numberOfBits) - 1;
			float encodedVal = ReadUInt32(numberOfBits);
			float unit = encodedVal / maxVal;
			return min + (unit * range);
		}

		/// <summary>
		/// Reads a 32 bit integer value written using WriteRangedInteger()
		/// </summary>
		/// <param name="min">The minimum value used when writing the value</param>
		/// <param name="max">The maximum value used when writing the value</param>
		/// <returns>A signed integer value larger or equal to MIN and smaller or equal to MAX</returns>
		public int ReadRangedInteger(int min, int max)
		{
			uint range = (uint)(max - min);
			int numBits = BitsToHoldUInt(range);

			uint rvalue = ReadUInt32(numBits);
			return (int)(min + rvalue);
		}

        /// <summary>
        /// Returns how many bits are necessary to hold a certain number
        /// </summary>
        [CLSCompliant(false)]
        public static int BitsToHoldUInt(uint value)
        {
            int bits = 1;
            while ((value >>= 1) != 0)
                bits++;
            return bits;
        }

		/// <summary>
		/// Reads a string written using Write(string)
		/// </summary>
		public string ReadString()
		{
			int byteLen = (int)ReadVariableUInt32();

			if (byteLen <= 0)
				return String.Empty;

			if ((ulong)(m_bitLength - m_readPosition) < ((ulong)byteLen * 8))
			{
				// not enough data
#if DEBUG
				
				throw new NetException(c_readOverflowError);
#else
				m_readPosition = m_bitLength;
				return null; // unfortunate; but we need to protect against DDOS
#endif
			}

			if ((m_readPosition & 7) == 0)
			{
				// read directly
				string retval = System.Text.Encoding.UTF8.GetString(m_data, m_readPosition >> 3, byteLen);
				m_readPosition += (8 * byteLen);
				return retval;
			}

			byte[] bytes = ReadBytes(byteLen);
			return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Reads a string written using Write(string) and returns true for success
		/// </summary>
		public bool ReadString(out string result)
		{
			uint byteLen;
			if (ReadVariableUInt32(out byteLen) == false)
			{
				result = String.Empty;
				return false;
			}

			if (byteLen <= 0)
			{
				result = String.Empty;
				return true;
			}

			if (m_bitLength - m_readPosition < (byteLen * 8))
			{
				result = String.Empty;
				return false;
			}

			if ((m_readPosition & 7) == 0)
			{
				// read directly
				result = System.Text.Encoding.UTF8.GetString(m_data, m_readPosition >> 3, (int)byteLen);
				m_readPosition += (8 * (int)byteLen);
				return true;
			}

			byte[] bytes;
			if (ReadBytes((int)byteLen, out bytes) == false)
			{
				result = String.Empty;
				return false;
			}

			result = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			return true;
		}
        
		/// <summary>
		/// Reads a stored IPv4 endpoint description
		/// </summary>
		public IPEndPoint ReadIPEndPoint()
		{
			byte len = ReadByte();
			byte[] addressBytes = ReadBytes(len);
			int port = ReadUInt16();

			IPAddress address = new IPAddress(addressBytes);
			return new IPEndPoint(address, port);
		}

		/// <summary>
		/// Pads data with enough bits to reach a full byte. Decreases cpu usage for subsequent byte writes.
		/// </summary>
		public void SkipPadBits()
		{
			m_readPosition = ((m_readPosition + 7) >> 3) * 8;
		}

		/// <summary>
		/// Pads data with enough bits to reach a full byte. Decreases cpu usage for subsequent byte writes.
		/// </summary>
		public void ReadPadBits()
		{
			m_readPosition = ((m_readPosition + 7) >> 3) * 8;
		}

		/// <summary>
		/// Pads data with the specified number of bits.
		/// </summary>
		public void SkipPadBits(int numberOfBits)
		{
			m_readPosition += numberOfBits;
		}
	}
}
#endif