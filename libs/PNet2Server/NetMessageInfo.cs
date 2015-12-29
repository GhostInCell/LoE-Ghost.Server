using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNet;

namespace PNetS
{
    public class PlayerMessageInfo
    {
        /// <summary>
        /// Whether or not to continue sending the message to all other players
        /// </summary>
        public bool ContinueForwarding;
        /// <summary>
        /// Mode the rpc was sent from by the client
        /// </summary>
        public readonly BroadcastMode Mode;

        /// <summary>
        /// Reliability mode of the message. You can change this to change the reliability as its passing through.
        /// </summary>
        public ReliabilityMode Reliability;

        internal PlayerMessageInfo(BroadcastMode mode)
        {
            Mode = mode;
        }
    }

    public class RoomMessageInfo
    {
        public bool ContinueForwarding;
        public readonly BroadcastMode Mode;
        public ReliabilityMode Reliability;

        internal RoomMessageInfo(BroadcastMode mode)
        {
            Mode = mode;
        }
    }
}
