using PNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace PNetR.Impl
{
    public class TcpDispatchClient : ADispatchClient
    {
        private TcpClient _serverClient;
        private NetworkStream _stream;

        private readonly Queue<NetMessage> _incomingMessages = new Queue<NetMessage>();
        private bool _shutdownQueued;
        private Thread _thread;
        private Thread _pingThread;

        protected internal override void Setup()
        {
        }

        protected internal override void Connect()
        {
            UpdateConnectionStatus(ConnectionStatus.Connecting);
            _serverClient = new TcpClient();
            _serverClient.BeginConnect(Room.Configuration.DispatcherAddress, Room.Configuration.DispatcherPort, ConnectCallback, _serverClient);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            client.ReceiveBufferSize = 1024;
            client.SendBufferSize = 1024;
            client.NoDelay = true;
            client.LingerState = new LingerOption(true, 10);

            if (client != _serverClient)
            {
                throw new Exception("TcpClient Mismatch");
            }

            try
            {
                client.EndConnect(ar);
            }
            catch (Exception e)
            {
                client.Close();
                Disconnected();
                UpdateConnectionStatus(ConnectionStatus.Disconnected);
                return;
            }

            _thread = new Thread(MessageLoop) { IsBackground = true, Name = "TcpDispatchClient MessageLoop" };
            _thread.Start(client);
        }

        private void MessageLoop(object state)
        {
            var client = state as TcpClient;
            try
            {
                
                Debug.Log("Authenticating with server");

                using (var networkStream = client.GetStream())
                {
                    _stream = networkStream;
                    PerformHail(networkStream);

                    var buffer = new byte[1024];
                    NetMessage dataBuffer = null;
                    var bufferSize = 0;
                    var lengthBuffer = new byte[2];
                    var bytesReceived = 0;
                    int readBytes;

                    var authMessages = new Queue<NetMessage>();
                    while (!_shutdownQueued)
                    {
                        readBytes = networkStream.Read(buffer, 0, buffer.Length);
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
                        if (!authMsg.ReadString(out var reason))
                            reason = "Not authorized";
                        Debug.LogError($"Could not connect to server. {reason}");
                        UpdateConnectionStatus(ConnectionStatus.FailedToConnect);
                        return;
                    }
                    if (!authMsg.ReadGuid(out var guid))
                        throw new Exception("Could not read room guid");

                    Connected(guid, client);
                    UpdateConnectionStatus(ConnectionStatus.Connected);

                    //and drain the rest of messages that might have come after the auth
                    while (authMessages.Count > 0)
                        EnqueueMessage(authMessages.Dequeue());

                    while (!_shutdownQueued)
                    {
                        readBytes = networkStream.Read(buffer, 0, buffer.Length);
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
                //if (!_shutdownQueued)
                //    Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
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
                Disconnected();
                UpdateConnectionStatus(ConnectionStatus.Disconnected);
            }
        }

        private void PerformHail(NetworkStream networkStream)
        {
            var hailMsg = GetMessage(100);
            hailMsg.Write(Room.Configuration.RoomIdentifier);

            //auth data
            hailMsg.Write((int)Room.Configuration.RoomAuthType);
            hailMsg.Write(Room.Configuration.AuthData);
            hailMsg.Write(Room.Configuration.UserDefinedAuthData);

            //connection info
            hailMsg.Write(Room.Configuration.ListenPort);
            hailMsg.Write(Room.Configuration.MaximumPlayers);
            if (Room.Configuration.ListenAddress != null && Room.Configuration.ListenAddress.Trim() != string.Empty)
            {
                hailMsg.Write(Room.Configuration.ListenAddress);
            }

            hailMsg.WriteSize();

            networkStream.Write(hailMsg.Data, 0, hailMsg.LengthBytes);
        }

        private void EnsureConnected()
        {
            while (!_shutdownQueued)
            {
                Thread.Sleep(5000);
                var msg = GetMessage(2);
                msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
                msg.Write(DandRRpcs.Ping);
                SendMessage(msg, ReliabilityMode.Ordered);
            }
        }

        private void EnqueueMessage(NetMessage msg)
        {
            lock (_incomingMessages)
                _incomingMessages.Enqueue(msg);
        }

        protected internal override void ReadQueue()
        {
            if (Room.Server == null) return;
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
                ConsumeData(msg);
                NetMessage.RecycleMessage(msg);
            }
        }

        protected internal override void Disconnect(string reason)
        {
            var server = Room.Server;
            if (server != null)
            {
                var msg = GetMessage(reason.Length * 2 + 5);
                msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
                msg.Write(DandRRpcs.DisconnectMessage);
                msg.Write(reason);
                SendMessage(msg, ReliabilityMode.Ordered);
            }
            _serverClient.Close();
        }

        protected internal override NetMessage GetMessage(int size)
        {
            return NetMessage.GetMessageSizePad(size);
        }

        protected internal override void SendMessage(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            try
            {

                _stream.BeginWrite(msg.Data, 0, msg.LengthBytes, EndWrite, msg);
            }
            catch (ObjectDisposedException ode)
            {
                if (ode.ObjectName != "System.Net.Sockets.NetworkStream")
                    Debug.LogException(ode, $"{ode.ObjectName} disposed when it shouldn't have");
            }
        }

        private void EndWrite(IAsyncResult ar)
        {
            var msg = ar.AsyncState as NetMessage;

            try
            {
                _stream.EndWrite(ar);
                if (!_serverClient.Connected)
                    _serverClient.Close();
            }
            catch (ObjectDisposedException ode)
            {
                if (ode.ObjectName != "System.Net.Sockets.NetworkStream")
                    Debug.LogException(ode, $"{ode.ObjectName} disposed when it shouldn't have");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _serverClient.Close();
            }
            finally
            {
                NetMessage.RecycleMessage(msg);
            }
        }
    }
}
