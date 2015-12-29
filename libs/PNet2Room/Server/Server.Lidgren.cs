#if S_LIDGREN
using Lidgren.Network;
using PNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNetR
{
    public partial class Server
    {
        internal NetConnection Connection { get; set; }

        partial void ImplementationSendMessage(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _room.DispatchClient.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            var method = mode.RoomDelivery();
            _room.DispatchClient.SendMessage(lmsg, method);
        }
    }
}
#endif