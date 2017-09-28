﻿/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

// Uncomment the line below to get statistics in RELEASE builds
//#define USE_RELEASE_STATISTICS

using System.Diagnostics;
using System.Text;

namespace Lidgren.Network
{
    internal enum MessageResendReason
	{
		Delay,
		HoleInSequence
	}

	/// <summary>
	/// Statistics for a NetConnection instance
	/// </summary>
	public sealed class NetConnectionStatistics
	{
		private readonly NetConnection m_connection;

		internal long m_sentPackets;
		internal long m_receivedPackets;

		internal long m_sentMessages;
		internal long m_receivedMessages;
		internal long m_droppedMessages;
		internal long m_receivedFragments;

		internal long m_sentBytes;
		internal long m_receivedBytes;

		internal long m_resentMessagesDueToDelay;
		internal long m_resentMessagesDueToHole;

		internal NetConnectionStatistics(NetConnection conn)
		{
			m_connection = conn;
			Reset();
		}

		internal void Reset()
		{
			m_sentPackets = 0;
			m_receivedPackets = 0;
			m_sentMessages = 0;
			m_receivedMessages = 0;
			m_receivedFragments = 0;
			m_sentBytes = 0;
			m_receivedBytes = 0;
			m_resentMessagesDueToDelay = 0;
			m_resentMessagesDueToHole = 0;
		}

		/// <summary>
		/// Gets the number of sent packets for this connection
		/// </summary>
		public long SentPackets { get { return m_sentPackets; } }

		/// <summary>
		/// Gets the number of received packets for this connection
		/// </summary>
		public long ReceivedPackets { get { return m_receivedPackets; } }

		/// <summary>
		/// Gets the number of sent bytes for this connection
		/// </summary>
		public long SentBytes { get { return m_sentBytes; } }

		/// <summary>
		/// Gets the number of received bytes for this connection
		/// </summary>
		public long ReceivedBytes { get { return m_receivedBytes; } }

        /// <summary>
        /// Gets the number of sent messages for this connection
        /// </summary>
        public long SentMessages { get { return m_sentMessages; } }

        /// <summary>
        /// Gets the number of received messages for this connection
        /// </summary>
        public long ReceivedMessages { get { return m_receivedMessages; } }

		/// <summary>
		/// Gets the number of resent reliable messages for this connection
		/// </summary>
		public long ResentMessages { get { return m_resentMessagesDueToHole + m_resentMessagesDueToDelay; } }

        /// <summary>
        /// Gets the number of dropped messages for this connection
        /// </summary>
        public long DroppedMessages { get { return m_droppedMessages; } }

		// public double LastSendRespondedTo { get { return m_connection.m_lastSendRespondedTo; } }

		[Conditional("DEBUG"), Conditional("USE_RELEASE_STATISTICS")]
        internal void PacketSent(int numBytes, int numMessages)
		{
            NetException.Assert(numBytes > 0 && numMessages > 0);
            m_sentPackets++;
            m_sentBytes += numBytes;
            m_sentMessages += numMessages;
        }

        [Conditional("DEBUG"), Conditional("USE_RELEASE_STATISTICS")]
        internal void PacketReceived(int numBytes, int numMessages, int numFragments)
		{
			NetException.Assert(numBytes > 0 && numMessages > 0);
			m_receivedPackets++;
			m_receivedBytes += numBytes;
			m_receivedMessages += numMessages;
			m_receivedFragments += numFragments;
		}

        [Conditional("DEBUG"), Conditional("USE_RELEASE_STATISTICS")]
        internal void MessageResent(MessageResendReason reason)
		{
			if (reason == MessageResendReason.Delay)
				m_resentMessagesDueToDelay++;
			else
				m_resentMessagesDueToHole++;
		}

        [Conditional("DEBUG"), Conditional("USE_RELEASE_STATISTICS")]
        internal void MessageDropped()
		{
			m_droppedMessages++;
		}

		/// <summary>
		/// Returns a string that represents this object
		/// </summary>
		public override string ToString()
		{
			StringBuilder bdr = new StringBuilder();
			//bdr.AppendLine("Average roundtrip time: " + NetTime.ToReadable(m_connection.m_averageRoundtripTime));
			bdr.AppendLine("Current MTU: " + m_connection.m_currentMTU);
			bdr.AppendLine("Sent " + m_sentBytes + " bytes in " + m_sentMessages + " messages in " + m_sentPackets + " packets");
			bdr.AppendLine("Received " + m_receivedBytes + " bytes in " + m_receivedMessages + " messages (of which " + m_receivedFragments + " fragments) in " + m_receivedPackets + " packets");
			bdr.AppendLine("Dropped " + m_droppedMessages + " messages (dupes/late/early)");

			if (m_resentMessagesDueToDelay > 0)
				bdr.AppendLine("Resent messages (delay): " + m_resentMessagesDueToDelay);
			if (m_resentMessagesDueToHole > 0)
				bdr.AppendLine("Resent messages (holes): " + m_resentMessagesDueToHole);

			int numUnsent = 0;
			int numStored = 0;
			foreach (NetSenderChannelBase sendChan in m_connection.m_sendChannels)
			{
				if (sendChan == null)
					continue;
				numUnsent += sendChan.QueuedSendsCount;

                if (sendChan is NetReliableSenderChannel relSendChan)
                {
					for (int i = 0; i < relSendChan.m_storedMessages.Length; i++)
						if ((relSendChan.m_usedStoredMessages & ((ulong)1 << i)) != 0)
							numStored++;
				}
			}

			int numWithheld = 0;
			foreach (NetReceiverChannelBase recChan in m_connection.m_receiveChannels)
			{
                if (recChan is NetReliableOrderedReceiver relRecChan)
                {
					for (int i = 0; i < relRecChan.m_withheldMessages.Length; i++)
						if (relRecChan.m_withheldMessages[i] != null)
							numWithheld++;
				}
			}

			bdr.AppendLine("Unsent messages: " + numUnsent);
			bdr.AppendLine("Stored messages: " + numStored);
			bdr.AppendLine("Withheld messages: " + numWithheld);

			return bdr.ToString();
		}
	}
}