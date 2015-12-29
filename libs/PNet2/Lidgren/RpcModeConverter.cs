using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if LIDGREN
using Lidgren.Network;
#endif

namespace PNet
{
    internal static class RpcModeConverter
    {
#if LIDGREN
        public static NetDeliveryMethod PlayerDelivery(this ReliabilityMode mode, out int sequenceChannel)
        {
            switch (mode)
            {
                case ReliabilityMode.Unordered:
                    sequenceChannel = 0;
                    return NetDeliveryMethod.ReliableUnordered;
                case ReliabilityMode.Unreliable:
                    sequenceChannel = 0;
                    return NetDeliveryMethod.Unreliable;
                default:
                    sequenceChannel = 2;
                    return NetDeliveryMethod.ReliableOrdered;
            }
        }

        public static NetDeliveryMethod RoomDelivery(this ReliabilityMode mode)
        {
            switch (mode)
            {
                case ReliabilityMode.Unordered:
                    return NetDeliveryMethod.ReliableUnordered;
                case ReliabilityMode.Unreliable:
                    return NetDeliveryMethod.Unreliable;
                default:
                    return NetDeliveryMethod.ReliableOrdered;
            }
        }

        public static void NetviewDelivery(RpcMode mode, out NetDeliveryMethod method, out int sequenceChannel)
        {
            switch (mode)
            {
                case RpcMode.AllBuffered:
                    method = NetDeliveryMethod.ReliableOrdered;
                    sequenceChannel = 5;
                    return;
                case RpcMode.OthersBuffered:
                    method = NetDeliveryMethod.ReliableOrdered;
                    sequenceChannel = 6;
                    return;
                case RpcMode.AllOrdered:
                    method = NetDeliveryMethod.ReliableOrdered;
                    sequenceChannel = 7;
                    return;
                case RpcMode.OthersOrdered:
                    method = NetDeliveryMethod.ReliableOrdered;
                    sequenceChannel = 8;
                    return;
                case RpcMode.AllUnordered:
                case RpcMode.OthersUnordered:
                case RpcMode.OwnerUnordered:
                    method = NetDeliveryMethod.ReliableUnordered;
                    sequenceChannel = 0;
                    return;
                case RpcMode.AllUnreliable:
                case RpcMode.OthersUnreliable:
                case RpcMode.OwnerUnreliable:
                    method = NetDeliveryMethod.Unreliable;
                    sequenceChannel = 0;
                    return;
                case RpcMode.OwnerBuffered:
                    method = NetDeliveryMethod.ReliableOrdered;
                    sequenceChannel = 9;
                    return;
                case RpcMode.OwnerOrdered:
                    sequenceChannel = 10;
                    method = NetDeliveryMethod.ReliableOrdered;
                    return;
                default:
                    sequenceChannel = 11;
                    method = NetDeliveryMethod.ReliableOrdered;
                    return;
            }
        }
#endif
    }
}