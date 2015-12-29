using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using PNet;
#if TCP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PNetS
{
    partial class Server
    {
        private TcpListener _roomListener;
        private TcpListener _clientListener;
        private CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();

        private bool _acceptEnabled;
        private bool _tcpRunning;

        partial void InternalInitialize()
        {
            _acceptEnabled = true;
            _tcpRunning = true;
            RoomListenerLoop();
            PlayerListenerLoop();
        }

        private async void PlayerListenerLoop()
        {
            _clientListener = new TcpListener(IPAddress.Any, Configuration.PlayerListenPort);
            _clientListener.Start();

            while (_acceptEnabled)
            {
                try
                {
                    var client = await _clientListener.AcceptTcpClientAsync();
                    HandleClientConnection(client);
                }
                catch (ObjectDisposedException ode)
                {
                    if (_acceptEnabled)
                    {
                        Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
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
            _roomListener = new TcpListener(IPAddress.Any, Configuration.RoomListenPort);
            _roomListener.Start();

            while (_acceptEnabled)
            {
                try
                {
                    var client = await _roomListener.AcceptTcpClientAsync();
                    HandleRoomConnection(client);
                }
                catch (ObjectDisposedException ode)
                {
                    if (_acceptEnabled)
                    {
                        Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
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
            Debug.Log("Room connection request from {0}", endpoint);
            client.ReceiveBufferSize = 1024;
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
                        !ApproveRoomConnection((IPEndPoint) client.Client.RemoteEndPoint,
                            authMessages.Count > 0 ? authMessages.Dequeue() : null, out denyReason, out room))
                    {
                        var dcMessage = GetMessage(denyReason.Length*2 + 1);
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

                    room.TcpClient = client;
                    room.NetworkStream = networkStream;

                    AddRoom(room);
                    UpdateRoomsOfNewRoom(room);

                    Debug.LogWarning("Room {1} connected from {0}, connectable at {2}", client.Client.RemoteEndPoint,
                        room.RoomId, room.Address);

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
                    Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
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
                Debug.Log("Closing room connection to {0}", endpoint);
                client.Close();

                if (room != null)
                {
                    RemoveRoom(room);
                    room.NetworkStream = null;
                    //todo: move all players in the room to a new room.
                }
            }
        }

        private async void HandleClientConnection(TcpClient client)
        {
            var endpoint = client.Client.RemoteEndPoint;
            Debug.Log("Player connection request from {0}", endpoint);
            client.ReceiveBufferSize = 1024;
            client.SendBufferSize = 1024;
            client.NoDelay = true;
            client.LingerState = new LingerOption(true, 10);

            Player player = null;
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

                    player = new Player(this)
                    {
                        TcpClient = client,
                        NetworkStream = networkStream,
                        Status = ConnectionStatus.Connecting
                    };
                    if (ConstructNetData != null)
                        player.NetUserData = ConstructNetData();

                    if (VerifyPlayer != null)
                    {
                        if (authMessages.Count > 0)
                            VerifyPlayer(player, authMessages.Dequeue());
                        else
                            player.Disconnect("No authentication message sent");
                    }
                    else
                    {
                        player.AllowConnect();
                    }

                    var canConnect = await player.AllowConnectCompletion.Task;
                    if (!canConnect) return;

                    player.Status = ConnectionStatus.Connected;
                    FinalizePlayerAdd(player);

                    Debug.Log("Client connected from {0}", client.Client.RemoteEndPoint);
                    //and drain the rest of messages that might have come after the auth
                    while (authMessages.Count > 0)
                        player.ConsumeData(authMessages.Dequeue());

                    while (_tcpRunning)
                    {
                        readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _shutdownTokenSource.Token);
                        if (readBytes > 0)
                        {
                            NetMessage.GetMessages(buffer, readBytes, ref bytesReceived, ref dataBuffer,
                                ref lengthBuffer, ref bufferSize, player.ConsumeData);
                        }
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
                    Debug.LogException(ode, "{0} disposed when it shouldn't have", ode.ObjectName);
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
                player.Status = ConnectionStatus.Disconnecting;
                Debug.Log("Closing client connection to {0}", endpoint);
                client.Close();
                if (player != null)
                {
                    player.Status = ConnectionStatus.Disconnected;
                    RemovePlayer(player);
                    player.NetworkStream = null;
                }
            }
        }

        partial void ImplSendToAllPlayers(NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            Player[] players;
            lock (_players)
                players = _players.Values;
            foreach (var player in players)
            {
                if (player == null) continue;
                player.SendTcpReadyMessage(msg);
            }
        }

        partial void ImplSendToAllPlayersExcept(Player player, NetMessage msg, ReliabilityMode mode)
        {
            msg.WriteSize();
            Player[] players;
            lock (_players)
                players = _players.Values;
            foreach (var pl in players)
            {
                if (pl == null) continue;
                if (pl == player) continue;
                pl.SendTcpReadyMessage(msg);
            }
        }

        partial void ImplementationShutdown(string reason)
        {
            _acceptEnabled = false;
            _roomListener.Stop();
            _clientListener.Stop();
            _shutdownTokenSource.Cancel();

            var shutReason = GetMessage(reason.Length*2 + 6);
            shutReason.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Owner, MsgType.Internal));
            shutReason.Write(DandRRpcs.DisconnectMessage);
            shutReason.Write(reason);
            shutReason.WriteSize();

            foreach (var room in _rooms.ToArray())
            {
                room.Value.SendTcpReadyMessage(shutReason);
            }
            Player[] players;
            lock (_players)
            {
                players = _players.Values;
            }
            foreach (var player in players)
            {
                player.SendTcpReadyMessage(shutReason);
            }
            _tcpRunning = false;
        }

        partial void ImplGetMessage(int length, ref NetMessage message)
        {
            message = NetMessage.GetMessageSizePad(length);
        }
    }
}
#endif