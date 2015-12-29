#if LIDGREN
using Lidgren.Network;
using PNet;

namespace PNetS
{
    public partial class Player
    {
        internal NetConnection Connection;

        partial void ImplSend(NetMessage msg, ReliabilityMode mode, bool recycle)
        {
            var lmsg = Server.PlayerServer.CreateMessage(msg.Data.Length);
            msg.Clone(lmsg);
            if (recycle)
                NetMessage.RecycleMessage(msg);

            int seq;
            var method = mode.PlayerDelivery(out seq);
            Server.PlayerServer.SendMessage(lmsg, Connection, method, seq);
        }

        partial void ImplementationAllowConnect()
        {
            var lmsg = Server.PlayerServer.CreateMessage(2);
            lmsg.Write(Id);
            Connection.Approve(lmsg);
        }

        partial void ImplementationDisconnect(string reason)
        {
            Connection.Disconnect(reason);
        }
    }
}
#endif