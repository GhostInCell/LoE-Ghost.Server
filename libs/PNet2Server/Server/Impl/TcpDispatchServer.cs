using PNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PNetS.Impl
{
    public class TcpDispatchServer : ADispatchServer
    {
        private TcpListener _roomListener;
        private TcpListener _clientListener;
        private CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();

        private bool _acceptEnabled;
        private bool _tcpRunning;
        private readonly List<ConnectionInfo> _players = new List<ConnectionInfo>();
        private readonly List<ConnectionInfo> _rooms = new List<ConnectionInfo>();

        protected internal override void Initialize()
        {
            _acceptEnabled = true;
            _tcpRunning = true;
            RoomListenerLoop();
            PlayerListenerLoop();
        }

        private async void PlayerListenerLoop()
        {
            _clientListener = new TcpListener(IPAddress.Any, Server.Configuration.PlayerListenPort);
            _clientListener.Start();

            while (_acceptEnabled)
            {
                try
                {
                    var client = await _clientListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    HandleClientConnection(client);
                }
                catch (ObjectDisposedException ode)
                {
                    if (_acceptEnabled)
                    {
                        Debug.LogException(ode, $"{ode.ObjectName} disposed when it shouldn't have");
                    }
                    //not worried about the _clientListener being disposed...
                }
                catch (Exception exp)
                {
                    Debug.LogException(exp);
                }

            }
        }

        private async void RoomListenerLoop()
        {
            _roomListener = new TcpListener(IPAddress.Any, Server.Configuration.RoomListenPort);
            _roomListener.Start();

            while (_acceptEnabled)
            {
                try
                {
                    var client = await _roomListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    HandleRoomConnection(client);
                }
                catch (ObjectDisposedException ode)
                {
                    if (_acceptEnabled)
                    {
                        Debug.LogException(ode, $"{ode.ObjectName} disposed when it shouldn't have");
                    }
                    //not worried about the _roomListener being disposed
                }
                catch (Exception exp)
                {
                    Debug.LogException(exp);
                }

            }
        }

        private async void HandleRoomConnection(TcpClient client)
        {
            var endpoint = client.Client.RemoteEndPoint;
            Debug.Log($"Room connection request from {endpoint}");
            client.ReceiveBufferSize = 1024;
            client.ReceiveTimeout = 5000;
            client.SendBufferSize = 1024;
            client.NoDelay = true;
            client.LingerState = new LingerOption(true, 10);

            Room room = null;
            try
            {
                using (var networkStream = client.GetStream())
                {
                    var buffer = new byte[1024];
                    NetMessage dataBuffer = null;
                    var bufferSize = 0;
                    var lengthBuffer = new byte[2];
                    var bytesReceived = 0;
                    int readBytes;

                    var authMessages = new Queue<NetMessage>();
                    while (_tcpRunning)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _shutdownTokenSource.Token);
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

                    string denyReason;
                    if (
                        !ApproveRoomConnection((IPEndPoint)client.Client.RemoteEndPoint,
                            authMessages.Count > 0 ? authMessages.Dequeue() : null, out denyReason, out room))
                    {
                        var dcMessage = GetMessage(denyReason.Length * 2 + 1);
                        dcMessage.Write(false);
                        dcMessage.WritePadBits();
                        dcMessage.Write(denyReason);
                        dcMessage.WriteSize();
                        await
                            networkStream.WriteAsync(dcMessage.Data, 0, dcMessage.LengthBytes,
                                _shutdownTokenSource.Token);
                        return;
                    }
                    else
                    {
                        var cMessage = GetMessage(17);
                        cMessage.Write(true);
                        cMessage.WritePadBits();
                        cMessage.Write(room.Guid);
                        cMessage.WriteSize();
                        await
                            networkStream.WriteAsync(cMessage.Data, 0, cMessage.LengthBytes, _shutdownTokenSource.Token);
                    }

                    var ci = new ConnectionInfo{Client = client, Stream = networkStream, Room = room};
                    lock (_rooms)
                        _rooms.Add(ci);

                    room.Connection = ci;
                    
                    AddRoom(room);

                    while (authMessages.Count > 0)
                        room.ConsumeData(authMessages.Dequeue());

                    while (_tcpRunning && room.Running)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _shutdownTokenSource.Token);
                        if (readBytes > 0)
                        {
                            NetMessage.GetMessages(buffer, readBytes, ref bytesReceived, ref dataBuffer,
                                ref lengthBuffer, ref bufferSize, room.ConsumeData);
                        }
                        else
                            EnsureConnected(ci);
                        if (!client.Connected)
                            return;
                    }
                }
            }
            catch (OperationCanceledException o)
            {
                Debug.Log("Operation canceled");
            }
            catch (ObjectDisposedException ode)
            {
                if (_tcpRunning && ode.ObjectName != "System.Net.Sockets.NetworkStream")
                    Debug.LogException(ode, $"{ode.ObjectName} disposed when it shouldn't have");
            }
            catch (SocketException se)
            {

            }
            catch (IOException ioe)
            {
                if (!(ioe.InnerException is SocketException))
                    Debug.LogException(ioe);
            }
            catch (Exception exp)
            {
                Debug.LogException(exp);
            }
            finally
            {
                Debug.Log($"Closing room connection to {endpoint}");
                client.Close();

                if (room != null)
                {
                    RemoveRoom(room);
                    lock (_rooms)
                        _rooms.Remove(room.Connection as ConnectionInfo);
                    room.Connection = null;
                    //todo: move all players in the room to a new room.
                }
            }
        }

        private async void HandleClientConnection(TcpClient client)
        {
            var endpoint = client.Client.RemoteEndPoint;
            Debug.Log($"Player connection request from {endpoint}");
            client.ReceiveBufferSize = 1024;
            client.ReceiveTimeout = 5000;
            client.SendBufferSize = 1024;
            client.NoDelay = true;
            client.LingerState = new LingerOption(true, 10);

            ConnectionInfo ci = null;
            try
            {
                using (var networkStream = client.GetStream())
                {
                    var buffer = new byte[1024];
                    NetMessage dataBuffer = null;
                    var bufferSize = 0;
                    var lengthBuffer = new byte[2];
                    var bytesReceived = 0;
                    int readBytes;

                    var authMessages = new Queue<NetMessage>();
                    while (_tcpRunning)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _shutdownTokenSource.Token);
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

                    ci = new ConnectionInfo
                    {
                        Client = client,
                        Stream = networkStream
                    };

                    lock (_players)
                        _players.Add(ci);

                    if (authMessages.Count > 0)
                        PlayerAttemptingConnection(ci, (IPEndPoint) client.Client.RemoteEndPoint, player =>
                        {
                            ci.Player = player;
                            player.Status = ConnectionStatus.Connecting;
                        }, authMessages.Dequeue());
                    else
                        ci.Player.Disconnect("No authentication message sent");

                    var canConnect = await ci.AllowConnectCompletion.Task;
                    if (!canConnect) return;

                    ci.Player.Status = ConnectionStatus.Connected;
                    FinalizePlayerAdd(ci.Player);

                    Debug.Log($"Client connected from {client.Client.RemoteEndPoint}");
                    //and drain the rest of messages that might have come after the auth
                    while (authMessages.Count > 0)
                        ci.Player.ConsumeData(authMessages.Dequeue());

                    while (_tcpRunning)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _shutdownTokenSource.Token);
                        if (readBytes > 0)
                        {
                            NetMessage.GetMessages(buffer, readBytes, ref bytesReceived, ref dataBuffer,
                                ref lengthBuffer, ref bufferSize, ci.Player.ConsumeData);
                        }
                        else
                            EnsureConnected(ci);
                        if (!client.Connected)
                            return;
                    }
                }
            }
            catch (OperationCanceledException o)
            {
                Debug.Log("Operation canceled");
            }
            catch (ObjectDisposedException ode)
            {
                if (_tcpRunning && !ode.ObjectName.StartsWith("System.Net.Sockets"))
                    Debug.LogException(ode, $"{ode.ObjectName} disposed when it shouldn't have");
            }
            catch (SocketException se)
            {

            }
            catch (IOException ioe)
            {
                if (!(ioe.InnerException is SocketException))
                    Debug.LogException(ioe);
            }
            catch (Exception exp)
            {
                Debug.LogException(exp);
            }
            finally
            {
                Debug.Log($"Closing client connection to {endpoint}");
                client.Close();

                if (ci != null)
                {
                    ci.Player.Status = ConnectionStatus.Disconnecting;
                
                    lock (_players)
                        _players.Remove(ci);
                    ci.Player.Status = ConnectionStatus.Disconnected;
                    RemovePlayer(ci.Player);
                    ci.Player.Connection = null;
                }
            }
        }

        private void EnsureConnected(ConnectionInfo connection)
        {
            var msg = GetMessage(2);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            msg.Write(DandPRpcs.Ping);
            SendTcpMessage(connection, msg);
        }

        protected internal override NetMessage GetMessage(int length)
        {
            return NetMessage.GetMessageSizePad(length);
        }

        protected internal override async Task Shutdown(string reason)
        {
            _acceptEnabled = false;
            _roomListener.Stop();
            _clientListener.Stop();
            _shutdownTokenSource.Cancel();

            var shutReason = GetMessage(reason.Length * 2 + 6);
            shutReason.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Owner, MsgType.Internal));
            shutReason.Write(DandRRpcs.DisconnectMessage);
            shutReason.Write(reason);
            shutReason.WriteSize();

            await Task.WhenAll(Task.WhenAll(LockRooms().Select(c => SendTcpMessage(c, shutReason, false))),
                Task.WhenAll(LockPlayers().Select(c => SendTcpMessage(c, shutReason, false))));

            NetMessage.RecycleMessage(shutReason);
            _tcpRunning = false;
        }

        protected internal override async void AllowPlayerToConnect(Player player)
        {
            var msg = Server.GetMessage(3);
            msg.Write(true);
            msg.WritePadBits();
            msg.Write(player.Id);
            msg.WriteSize();

            var ci = player.Connection as ConnectionInfo;
            try
            {
                var strm = ci.Stream;
                if (strm != null)
                    await ci.Stream.WriteAsync(msg.Data, 0, msg.LengthBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ci.Client.Close();
                ci.AllowConnectCompletion.TrySetResult(false);
                return;
            }
            finally
            {
                NetMessage.RecycleMessage(msg);
            }
            ci.AllowConnectCompletion.TrySetResult(true);
        }

        protected internal override async void Disconnect(Player player, string reason)
        {
            var msg = Server.GetMessage(reason.Length * 2 + 5);
            var ci = player.Connection as ConnectionInfo;
            if (ci.AllowConnectCompletion.TrySetResult(false))
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
                var strm = ci.Stream;
                if (strm != null)
                    await strm.WriteAsync(msg.Data, 0, msg.LengthBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            await Task.Delay(5000);
            NetMessage.RecycleMessage(msg);
            ci.Client.Close();
        }

        protected internal override void SendToPlayer(Player player, NetMessage msg, ReliabilityMode mode, bool recycle)
        {
            msg.WriteSize();
            SendTcpMessage(player.Connection as ConnectionInfo, msg, recycle);
        }

        protected internal override void SendToAllPlayers(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            var conns = LockPlayers();
            var tasks = new List<Task>(conns.Length);
            foreach (var c in conns)
            {
                tasks.Add(SendTcpMessage(c, msg, false));
            }
            Task.WhenAll(tasks).ContinueWith(task => NetMessage.RecycleMessage(msg));
        }

        ConnectionInfo[] LockPlayers()
        {
            lock (_players)
                return _players.ToArray();
        }

        ConnectionInfo[] LockRooms()
        {
            lock (_rooms)
                return _rooms.ToArray();
        }

        protected internal override void SendToAllPlayersExcept(Player player, NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            var conns = LockPlayers();
            var ci = player.Connection as ConnectionInfo;
            var tasks = new List<Task>(conns.Length);
            foreach (var pl in conns)
            {
                if (pl == null) continue;
                if (pl == ci) continue;
                tasks.Add(SendTcpMessage(pl, msg, false));
            }
            Task.WhenAll(tasks).ContinueWith(task => NetMessage.RecycleMessage(msg));
        }

        async internal Task SendTcpMessage(ConnectionInfo connection, NetMessage msg, bool recycle = true)
        {
            try
            {
                var strm = connection.Stream;
                if (strm == null)
                    return;
                await strm.WriteAsync(msg.Data, 0, msg.LengthBytes);
                if (!connection.Client.Connected)
                    connection.Client.Close();
            }
            catch (ObjectDisposedException oe)
            {
                if (oe.ObjectName == "System.Net.Sockets.NetworkStream")
                { }
                else
                {
                    Debug.LogException(oe);
                    connection.Client.Close();
                }
            }
            catch (IOException ioe)
            {
                if (!(ioe.InnerException is SocketException))
                    Debug.LogException(ioe, $"On {connection.User}");
                connection.Client.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e, $"On {connection.User}");
                connection.Client.Close();
            }
            finally
            {
                if (recycle)
                    NetMessage.RecycleMessage(msg);
            }
        }

        protected internal override void SendToRoom(Room room, NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            SendTcpMessage(room.Connection as ConnectionInfo, msg);
        }

        protected internal override void SendToOtherRooms(Room except, NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            var conns = LockRooms();
            var ci = except.Connection as ConnectionInfo;
            var tasks = new List<Task>(conns.Length);
            foreach (var c in conns)
            {
                if (c == null) continue;
                if (c == ci) continue;
                tasks.Add(SendTcpMessage(c, msg, false));
            }
            Task.WhenAll(tasks).ContinueWith(task => NetMessage.RecycleMessage(msg));
        }

        protected internal override void SendToAllRooms(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            var conns = LockRooms();
            var tasks = new List<Task>(conns.Length);
            foreach (var c in conns)
            {
                tasks.Add(SendTcpMessage(c, msg, false));
            }
            Task.WhenAll(tasks).ContinueWith(task => NetMessage.RecycleMessage(msg));
        }
    }

    class ConnectionInfo
    {
        public TcpClient Client;
        public NetworkStream Stream;
        public readonly TaskCompletionSource<bool> AllowConnectCompletion = new TaskCompletionSource<bool>();
        public Player Player;
        public Room Room;
        public object User { get { return (object) Player ?? Room; } }

        public override string ToString()
        {
            return Client.Client.RemoteEndPoint.ToString();
        }
    }
}
