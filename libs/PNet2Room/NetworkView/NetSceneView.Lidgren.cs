#if R_LIDGREN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using PNet;

namespace PNetR
{
    partial class NetworkedSceneObjectView
    {
        partial void ImplSendMessage(NetMessage msg, ReliabilityMode mode)
        {
            var lmsg = _room.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            if (seq == 2) seq = 3; //don't use player channel...

            _room.PlayerServer.SendToAll(lmsg, null, method, seq);
        }
    }
}
#endif