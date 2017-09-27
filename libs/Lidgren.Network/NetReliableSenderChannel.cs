﻿using System;
using System.Threading;

namespace Lidgren.Network
{
	/// <summary>
	/// Sender part of Selective repeat ARQ for a particular NetChannel
	/// </summary>
	internal sealed class NetReliableSenderChannel : NetSenderChannelBase
	{
		private NetConnection m_connection;
		private int m_windowStart;
		private int m_windowSize;
		private int m_sendStart;

		private NetBitVector m_receivedAcks;
		internal ulong m_usedStoredMessages; // "used" bits for storedMessages
		internal NetStoredReliableMessage[] m_storedMessages;

		internal double m_resendDelay;

		internal override int WindowSize { get { return m_windowSize; } }

		internal NetReliableSenderChannel(NetConnection connection, int windowSize)
		{
			m_connection = connection;
			m_windowSize = windowSize;
			m_windowStart = 0;
			m_sendStart = 0;
			m_receivedAcks = new NetBitVector(NetConstants.NumSequenceNumbers);
			NetException.Assert(m_windowSize <= 64); // we do only have sizeof(ulong)*8 "used" bits in m_usedStoredMessages
			m_usedStoredMessages = 0;
			m_storedMessages = new NetStoredReliableMessage[m_windowSize];
			m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
			m_resendDelay = m_connection.GetResendDelay();
		}

		internal override int GetAllowedSends()
		{
			int retval = m_windowSize - ((m_sendStart + NetConstants.NumSequenceNumbers) - m_windowStart) & NetConstants.NumSequenceNumberMask;
			NetException.Assert(retval >= 0 && retval <= m_windowSize);
			return retval;
		}

		internal override void Reset()
		{
			m_receivedAcks.Clear();
			for (int i = 0; i < m_storedMessages.Length; i++)
				m_storedMessages[i].Reset();
			m_queuedSends.Clear();
			m_windowStart = 0;
			m_sendStart = 0;
		}

		internal override NetSendResult Enqueue(NetOutgoingMessage message)
		{
			m_queuedSends.Enqueue(message);
			m_connection.m_peer.m_needFlushSendQueue = true; // a race condition to this variable will simply result in a single superflous call to FlushSendQueue()
			if (m_queuedSends.Count <= GetAllowedSends())
				return NetSendResult.Sent;
			return NetSendResult.Queued;
		}

        // call this regularely
        internal override void SendQueuedMessages(double now)
        {
            //
            // resends
            //
            if (m_usedStoredMessages != 0)
            {
                for (int i = 0; i < m_storedMessages.Length; i++)
                {
                    if ((m_usedStoredMessages & ((ulong)1 << i)) == 0)
                        continue;

                    var storedMsg = m_storedMessages[i];
                    NetOutgoingMessage om = storedMsg.Message;
                    if (om == null)
                        continue;

                    double t = storedMsg.LastSent;
                    if (t > 0 && (now - t) > m_resendDelay)
                    {
                        // deduce sequence number
                        /*
                        int startSlot = m_windowStart % m_windowSize;
                        int seqNr = m_windowStart;
                        while (startSlot != i)
                        {
                            startSlot--;
                            if (startSlot < 0)
                                startSlot = m_windowSize - 1;
                            seqNr--;
                        }
                        */

                        //m_connection.m_peer.LogVerbose("Resending due to delay #" + m_storedMessages[i].SequenceNumber + " " + om.ToString());
                        m_connection.m_statistics.MessageResent(MessageResendReason.Delay);

                        Interlocked.Increment(ref om.m_recyclingCount); // increment this since it's being decremented in QueueSendMessage
                        m_connection.QueueSendMessage(om, storedMsg.SequenceNumber);

                        m_storedMessages[i].LastSent = now;
                        m_storedMessages[i].NumSent++;
                    }
                }
            }

            int num = GetAllowedSends();
            if (num == 1)
                return;

            // queued sends
            int queued = m_queuedSends.Count;
            while (num > 0 && queued > 0)
            {
                if (m_queuedSends.TryDequeue(out var om))
                {
                    ExecuteSend(now, om);
                    queued--;
                }
                num--;
                NetException.Assert(num == GetAllowedSends());
            }
        }

		private void ExecuteSend(double now, NetOutgoingMessage message)
		{
			int seqNr = m_sendStart;
			m_sendStart = (m_sendStart + 1) & NetConstants.NumSequenceNumberMask;

			// must increment recycle count here, since it's decremented in QueueSendMessage and we want to keep it for the future in case or resends
			// we will decrement once more in DestoreMessage for final recycling
			Interlocked.Increment(ref message.m_recyclingCount);

			m_connection.QueueSendMessage(message, seqNr);

			int storeIndex = seqNr % m_windowSize;
			NetException.Assert((m_usedStoredMessages & ((ulong)1 << storeIndex)) == 0);

			m_usedStoredMessages |= (ulong)1 << storeIndex; // set used bit
			m_storedMessages[storeIndex].NumSent++;
			m_storedMessages[storeIndex].Message = message;
			m_storedMessages[storeIndex].LastSent = now;
			m_storedMessages[storeIndex].SequenceNumber = seqNr;

			return;
		}

		private void DestoreMessage(int storeIndex)
		{
			NetOutgoingMessage storedMessage = m_storedMessages[storeIndex].Message;

			// on each destore; reduce recyclingcount so that when all instances are destored, the outgoing message can be recycled
			Interlocked.Decrement(ref storedMessage.m_recyclingCount);
#if DEBUG
			if (storedMessage == null)
				throw new NetException("m_storedMessages[" + storeIndex + "].Message is null; sent " + m_storedMessages[storeIndex].NumSent + " times, last time " + (NetTime.Now - m_storedMessages[storeIndex].LastSent) + " seconds ago");
#else
			if (storedMessage != null)
			{
#endif
			if (storedMessage.m_recyclingCount <= 0)
				m_connection.m_peer.Recycle(storedMessage);

#if !DEBUG
			}
#endif
			m_usedStoredMessages &= ~((ulong)1 << storeIndex); // clear used bit
			m_storedMessages[storeIndex] = new NetStoredReliableMessage();
		}

		// remoteWindowStart is remote expected sequence number; everything below this has arrived properly
		// seqNr is the actual nr received
		internal override void ReceiveAcknowledge(double now, int seqNr)
		{
			// late (dupe), on time or early ack?
			int relate = NetUtility.RelativeSequenceNumber(seqNr, m_windowStart);

			if (relate < 0)
			{
				//m_connection.m_peer.LogDebug("Received late/dupe ack for #" + seqNr);
				return; // late/duplicate ack
			}

			if (relate == 0)
			{
				//m_connection.m_peer.LogDebug("Received right-on-time ack for #" + seqNr);

				// ack arrived right on time
				NetException.Assert(seqNr == m_windowStart);

				m_receivedAcks[m_windowStart] = false;
				DestoreMessage(m_windowStart % m_windowSize);
				m_windowStart = (m_windowStart + 1) & NetConstants.NumSequenceNumberMask;

				// advance window if we already have early acks
				while (m_receivedAcks.Get(m_windowStart))
				{
					//m_connection.m_peer.LogDebug("Using early ack for #" + m_windowStart + "...");
					m_receivedAcks[m_windowStart] = false;
					DestoreMessage(m_windowStart % m_windowSize);

					NetException.Assert((m_usedStoredMessages & ((ulong)1 << (m_windowStart % m_windowSize))) == 0); // should already be destored
					m_windowStart = (m_windowStart + 1) & NetConstants.NumSequenceNumberMask;
					//m_connection.m_peer.LogDebug("Advancing window to #" + m_windowStart);
				}

				return;
			}

			//
			// early ack... (if it has been sent!)
			//
			// If it has been sent either the m_windowStart message was lost
			// ... or the ack for that message was lost
			//

			//m_connection.m_peer.LogDebug("Received early ack for #" + seqNr);

			int sendRelate = NetUtility.RelativeSequenceNumber(seqNr, m_sendStart);
			if (sendRelate <= 0)
			{
				// yes, we've sent this message - it's an early (but valid) ack
				if (m_receivedAcks[seqNr])
				{
					// we've already destored/been acked for this message
				}
				else
				{
					m_receivedAcks[seqNr] = true;
				}
			}
			else if (sendRelate > 0)
			{
				// uh... we haven't sent this message yet? Weird, dupe or error...
				NetException.Assert(false, "Got ack for message not yet sent?");
				return;
			}

			// Ok, lets resend all missing acks
			int rnr = seqNr;
			do
			{
				rnr--;
				if (rnr < 0)
					rnr = NetConstants.NumSequenceNumbers - 1;

				if (m_receivedAcks[rnr])
				{
					// m_connection.m_peer.LogDebug("Not resending #" + rnr + " (since we got ack)");
				}
				else
				{
					int slot = rnr % m_windowSize;
					NetException.Assert(m_storedMessages[slot].Message != null);
					if (m_storedMessages[slot].NumSent == 1)
					{
						// just sent once; resend immediately since we found gap in ack sequence
						NetOutgoingMessage rmsg = m_storedMessages[slot].Message;
						//m_connection.m_peer.LogVerbose("Resending #" + rnr + " (" + rmsg + ")");

						if (now - m_storedMessages[slot].LastSent < (m_resendDelay * 0.35))
						{
							// already resent recently
						}
						else
						{
							m_storedMessages[slot].LastSent = now;
							m_storedMessages[slot].NumSent++;
							m_connection.m_statistics.MessageResent(MessageResendReason.HoleInSequence);
							Interlocked.Increment(ref rmsg.m_recyclingCount); // increment this since it's being decremented in QueueSendMessage
							m_connection.QueueSendMessage(rmsg, rnr);
						}
					}
				}

			} while (rnr != m_windowStart);
		}
	}
}
