using PNet;

namespace PNetR
{
    public class NetMessageInfo
    {
        public readonly Player Sender;
        public bool ContinueForwarding;
        /// <summary>
        /// Mode the rpc was sent from by the client
        /// </summary>
        public readonly BroadcastMode Mode;

        /// <summary>
        /// Reliability mode of the message. You can change this to change the reliability as its passing through.
        /// </summary>
        public ReliabilityMode Reliability;

        internal NetMessageInfo(BroadcastMode mode, Player sender)
        {
            Mode = mode;
            Sender = sender;
            ContinueForwarding = true;
        }
    }
}