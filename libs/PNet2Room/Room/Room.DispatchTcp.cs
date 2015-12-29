#if S_TCP
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using PNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNetR
{
    partial class Room
    {
        private TcpClient _serverClient;

        partial void ImplDispatchSetup()
        {
            _serverClient = new TcpClient();
        }

        partial void ImplDispatchConnect()
        {
            ServerStatus = ConnectionStatus.Connecting;
            _serverClient.BeginConnect(Configuration.DispatcherAddress, Configuration.DispatcherPort, ConnectCallback, _serverClient);
        }

        private async void ConnectCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            try
            {
                if (client != _serverClient)
                {
                    throw new Exception("TcpClient Mismatch");
                }

                client.EndConnect(ar);
                Debug.Log("Authenticating with server");

                using (var networkStream = client.GetStream())
                {
                    await PerformHail(networkStream);

                    var buffer = new byte[1024];
                    NetMessage dataBuffer = null;
                    var bufferSize = 0;
                    var lengthBuffer = new byte[2];
                    var bytesReceived = 0;
                    int readBytes;


                    var authMessages = new Queue<NetMessage>();
                    while (!_shutdownQueued)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        if (readBytes > 0)
                        {
                            var readMessage = NetMessage.GetMessages(buffer, readBytes, ref bytesReceived,
                                ref dataBuffer, ref lengthBuffer, ref bufferSize, authMessages.Enqueue);
                            if (readMessage >= 1)
                                break;
                        }
                        if (!client.Connected)
                            return;
                    }

                    if (authMessages.Count == 0)
                        throw new Exception("Could not read auth result");
                    var authMsg = authMessages.Dequeue();

                    var auth = authMsg.ReadBoolean();
                    authMsg.ReadPadBits();
                    if (!auth)
                    {
                        string reason;
                        if (!authMsg.ReadString(out reason))
                            reason = "Not authorized";
                        Debug.LogError("Could not connect to server. {0}", reason);
                        ServerStatus = ConnectionStatus.FailedToConnect;
                        ServerStatusChanged.TryRaise(Debug.Logger);
                        return;
                    }
                    Guid guid;
                    if (!authMsg.ReadGuid(out guid))
                        throw new Exception("Could not read room guid");

                    RoomId = guid;
                    Debug.Log("Connected to dispatcher. Id is {0}", RoomId);
                    Server = new Server(this) {NetworkStream = networkStream, TcpClient = client};
                    ServerStatus = ConnectionStatus.Connected;
                    ServerStatusChanged.TryRaise(Debug.Logger);

                    //and drain the rest of messages that might have come after the auth
                    while (authMessages.Count > 0)
                        EnqueueMessage(authMessages.Dequeue());

                    while (!_shutdownQueued)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        if (readBytes > 0)
                        {
                            NetMessage.GetMessages(buffer, readBytes, ref bytesReceived, ref dataBuffer,
                                ref lengthBuffer, ref bufferSize, EnqueueMessage);
                        }
                        if (!client.Connected)
                            return;
                    }

                }
            }
            catch (ObjectDisposedException ode)
            {
                if (!_shutdownQueued)
                    Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
            }
            catch (IOException ioe)
            {
                if (!(ioe.InnerException is SocketException))
                    Debug.LogException(ioe);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
                ServerStatus = ConnectionStatus.Disconnected;
                ServerStatusChanged.TryRaise(Debug.Logger);
            }
        }

        private Task PerformHail(NetworkStream networkStream)
        {
            var hailMsg = ServerGetMessage(100);
            hailMsg.Write(Configuration.RoomIdentifier);

            //auth data
            hailMsg.Write((int)Configuration.RoomAuthType);
            hailMsg.Write(Configuration.AuthData);
            hailMsg.Write(Configuration.UserDefinedAuthData);

            //connection info
            hailMsg.Write(_serverConfiguration.Port);
            hailMsg.Write(Configuration.MaximumPlayers);
            if (Configuration.ListenAddress != null && Configuration.ListenAddress.Trim() != string.Empty)
            {
                hailMsg.Write(Configuration.ListenAddress);
            }

            hailMsg.WriteSize();

            return networkStream.WriteAsync(hailMsg.Data, 0, hailMsg.LengthBytes);
        }

        private readonly Queue<NetMessage> _incomingMessages = new Queue<NetMessage>();
        private void EnqueueMessage(NetMessage msg)
        {
            lock (_incomingMessages)
                _incomingMessages.Enqueue(msg);
        }

        partial void ImplDispatchRead()
        {
            if (Server == null) return;
            while (true)
            {
                NetMessage msg;
                lock (_incomingMessages)
                {
                    if (_incomingMessages.Count > 0)
                        msg = _incomingMessages.Dequeue();
                    else
                        return;
                }
                Server.ConsumeData(msg);
                NetMessage.RecycleMessage(msg);
            }
        }

        partial void ImplDispatchDisconnect(string reason)
        {
            var server = Server;
            if (server != null)
            {
                var msg = ServerGetMessage(reason.Length*2 + 5);
                msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
                msg.Write(DandRRpcs.DisconnectMessage);
                msg.Write(reason);
                server.SendMessage(msg, ReliabilityMode.Ordered);
            }
            _serverClient.Close();
        }

        partial void ImplServerGetMessage(int size, ref NetMessage msg)
        {
            msg = NetMessage.GetMessageSizePad(size);
        }
    }
}
#endif