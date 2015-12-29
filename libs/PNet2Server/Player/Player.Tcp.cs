#if TCP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PNet;

namespace PNetS
{
    partial class Player
    {
        internal TcpClient TcpClient;
        internal NetworkStream NetworkStream;
        internal readonly TaskCompletionSource<bool> AllowConnectCompletion = new TaskCompletionSource<bool>();

        async partial void ImplSend(NetMessage msg, ReliabilityMode mode, bool recycle)
        {
            msg.WriteSize();
            try
            {
                var strm = NetworkStream;
                if (strm == null)
                    return;
                await strm.WriteAsync(msg.Data, 0, msg.LengthBytes);
                if (!TcpClient.Connected)
                    TcpClient.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                NetworkStream.Dispose();
                TcpClient.Close();
            }
            finally
            {
                if (recycle)
                    NetMessage.RecycleMessage(msg);
            }
        }

        async internal void SendTcpReadyMessage(NetMessage msg)
        {
            try
            {
                var strm = NetworkStream;
                if (strm == null)
                    return;
                await NetworkStream.WriteAsync(msg.Data, 0, msg.LengthBytes);
                if (!TcpClient.Connected)
                    TcpClient.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TcpClient.Close();
            }
        }

        async partial void ImplementationAllowConnect()
        {
            var msg = Server.GetMessage(3);
            msg.Write(true);
            msg.WritePadBits();
            msg.Write(Id);
            msg.WriteSize();

            try
            {
                var strm = NetworkStream;
                if (strm != null)
                    await NetworkStream.WriteAsync(msg.Data, 0, msg.LengthBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TcpClient.Close();
                AllowConnectCompletion.TrySetResult(false);
                return;
            }
            finally
            {
                NetMessage.RecycleMessage(msg);
            }
            AllowConnectCompletion.TrySetResult(true);
        }

        async partial void ImplementationDisconnect(string reason)
        {
            var msg = Server.GetMessage(reason.Length*2 + 5);
            if (AllowConnectCompletion.TrySetResult(false))
            {
                //we're actually denying connection, not sending a disconnect
                msg.Write(false);
                msg.WritePadBits();
                msg.Write(reason);
            }
            else
            {
                msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Owner, MsgType.Internal));
                msg.Write(DandPRpcs.DisconnectMessage);
                msg.Write(reason);
            }
            msg.WriteSize();
            try
            {
                var strm = NetworkStream;
                if (strm != null)
                    await strm.WriteAsync(msg.Data, 0, msg.LengthBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                NetMessage.RecycleMessage(msg);
                TcpClient.Close();
            }
        }
    }
}
#endif