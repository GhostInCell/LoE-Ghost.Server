using System.Net.Sockets;
#if TCP
using PNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PNetS
{
    partial class Room
    {
        internal TcpClient TcpClient;
        internal NetworkStream NetworkStream;

        async partial void ImplementationSendMessage(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            try
            {
                var strm = NetworkStream;
                if (strm != null)
                    await strm.WriteAsync(msg.Data, 0, msg.LengthBytes);
            }
            catch (Exception e)
            {
                TcpClient.Close();
            }
            NetMessage.RecycleMessage(msg);
        }

        partial void ImplSendMessageToOthers(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            foreach (var room in _server.Rooms.ToArray())
            {
                if (room.Value == null) continue;
                if (room.Value == this) continue;
                room.Value.SendTcpReadyMessage(msg);
            }
            NetMessage.RecycleMessage(msg);
        }

        partial void ImplSendToAll(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            foreach (var room in _server.Rooms.ToArray())
            {
                if (room.Value == null) continue;
                room.Value.SendTcpReadyMessage(msg);
            }
            NetMessage.RecycleMessage(msg);
        }

        async internal void SendTcpReadyMessage(NetMessage msg)
        {
            try
            {
                var strm = NetworkStream;
                if (strm != null)
                    await strm.WriteAsync(msg.Data, 0, msg.LengthBytes);
            }
            catch (Exception e)
            {
                TcpClient.Close();
            }
        }
    }
}
#endif