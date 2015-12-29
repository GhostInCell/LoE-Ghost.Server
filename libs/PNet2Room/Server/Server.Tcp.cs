#if S_TCP
using PNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PNetR
{
    public partial class Server
    {
        internal TcpClient TcpClient;
        internal NetworkStream NetworkStream;

        partial void ImplementationSendMessage(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            try
            {

                NetworkStream.BeginWrite(msg.Data, 0, msg.LengthBytes, EndWrite, msg);
            }
            catch (ObjectDisposedException ode)
            {
                if (ode.ObjectName != "System.Net.Sockets.NetworkStream")
                    Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
            }
        }

        private void EndWrite(IAsyncResult ar)
        {
            var msg = ar.AsyncState as NetMessage;

            try
            {
                NetworkStream.EndWrite(ar);
                if (!TcpClient.Connected)
                    TcpClient.Close();
            }
            catch (ObjectDisposedException ode)
            {
                if (ode.ObjectName != "System.Net.Sockets.NetworkStream")
                    Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TcpClient.Close();
            }
            finally
            {
                NetMessage.RecycleMessage(msg);
            }
        }
    }
}
#endif