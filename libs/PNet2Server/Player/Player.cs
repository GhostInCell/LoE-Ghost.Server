using System;
using System.Net;
using PNet;

namespace PNetS
{
    public partial class Player : IInfoRpcProvider<PlayerMessageInfo>, IProxySingle<IPlayerProxy>
    {
        public ushort Id { get; internal set; }

        /// <summary>
        /// the actual endpoint connection to the player
        /// </summary>
#if LIDGREN
        public IPEndPoint EndPoint { get { return Connection.RemoteEndPoint; } }
#elif TCP
        public IPEndPoint EndPoint { get { return TcpClient.Client.RemoteEndPoint as IPEndPoint; } }
#endif

        /// <summary>
        /// custom object to associate with the player. not synched over the network.
        /// </summary>
        public object UserData;
        private Guid _currentRoom = Guid.Empty;
        internal Guid CurrentRoomGuid{ get { return _currentRoom; }}
        private Guid _switchingToRoom = Guid.Empty;
        private Guid _oldRoom = Guid.Empty;

        /// <summary>
        /// Custom object to associate with the player, synchronizable over the network
        /// </summary>
        public INetSerializable NetUserData;
        /// <summary>
        /// return (T)NetUserData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T TnUser<T>() where T : INetSerializable
        {
            return (T)NetUserData;
        }
        /// <summary>
        /// Fired on NetUserData getting updated from the network
        /// </summary>
        public event Action<Player> NetUserDataChanged;

        public event Action<Room> FinishedSwitchingRooms;

        /// <summary>
        /// Fired when if the room this was switching to suddenly became invalid.
        /// </summary>
        public event Action<Player> SwitchingToRoomInvalidated;

        internal virtual void OnNetUserDataChanged()
        {
            NetUserDataChanged?.Invoke(this);
        }

        internal virtual void OnSwitchingToRoomInvalidated()
        {
            var ev = SwitchingToRoomInvalidated;
            if (ev == null)
            {
                Disconnect("Room disconnected");
                return;
            }

            try
            {
                ev(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Disconnect("Room disconnected");
            }
        }


        /// <summary>
        /// number of times the player has incurred an internal error
        /// </summary>
        public int InternalErrorCount
        {
            get { return _internalErrorCount; }
            internal set
            {
                _internalErrorCount = value;
                if (_internalErrorCount > MaxInternalErrorCount)
                {
                    Debug.LogWarning($"Player {UserData} disconnected for reaching maximum internal error count");
                    Disconnect("Maximum allowable network errors reached");
                }
            }
        }

        public ConnectionStatus Status { get; internal set; }

        private int _internalErrorCount;

        /// <summary>
        /// number of times a player can incur internal errors before being automatically disconnected
        /// </summary>
        public static int MaxInternalErrorCount = 100;
        /// <summary>
        /// the server this is associated with
        /// </summary>
        public readonly Server Server;
        internal Player(Server server)
        {
            Server = server;
        }

        internal Player(Server server, INetSerializable netUserData)
        {
            Server = server;
            NetUserData = netUserData;
        }

        /// <summary>
        /// a player that represents the server. 
        /// This player cannot have any methods called on it that involve the network (rpcs, changin rooms, approving, etc)
        /// </summary>
        public static readonly Player ServerPlayer = new Player(null);


        /// <summary>
        /// serialize the NetUserData over the network to the room that has this player
        /// </summary>
        /// <returns>false, if the data was not synchronized (the user isn't in a room, or NetUserData is null)</returns>
        public bool SynchNetData()
        {
#if DEBUG
            Debug.Log($"Syncing net data for {this}");
#endif
            if (NetUserData == null) return false;
            var msg = Server.GetMessage(NetUserData.AllocSize + 4);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            msg.Write(DandRRpcs.SyncNetUser);
            msg.Write(Id);
            NetUserData.OnSerialize(msg);

            var room = Room;
            if (room == null)
            {
                NetMessage.RecycleMessage(msg);
                return false;
            }

            room.SendMessage(msg, ReliabilityMode.Ordered);
            
            return true;
        }

        /// <summary>
        /// the current room the player is in. Null if not in any room.
        /// </summary>
        public Room Room
        {
            get { return Server.GetRoom(_currentRoom); }
        }

        public Guid SwitchingToRoom { get { return _switchingToRoom; } }

        /// <summary>
        /// switch the player to any of the rooms that match the room id (load balanced)
        /// </summary>
        /// <param name="roomId"></param>
        public bool ChangeRoom(string roomId)
        {
            if (this == ServerPlayer) return false;

            Room room;

            if (!Server.TryGetRooms(roomId, out var rooms))
            {
                Debug.LogWarning($"Could not determine room {roomId} for player {this} - no rooms exist");
                return false;
            }

            var deter = DetermineChangeRoom;
            if (deter != null)
            {
                room = deter(rooms);
                if (room != null)
                {
                    SendRoomSwitch(room);
                    return true;
                }
            }

            room = null;
            var c = int.MaxValue;
            foreach (var r in rooms)
            {
                if (r == null)
                    continue;
                if (r.PlayerCount >= c) continue;
                room = r;
                c = r.PlayerCount;
            }
            
            if (room == null)
            {
                Debug.LogWarning($"Could not determine room {roomId} for player {this}");
                return false;
            }
            
            SendRoomSwitch(room);
            return true;
        }

        /// <summary>
        /// if subscribed to, you can use this to control the load balancing. Returning null will make the load balancer act normally (the most empty room)
        /// </summary>
        public event Func<Room[], Room> DetermineChangeRoom;

        /// <summary>
        /// Switch the player to the specific room
        /// </summary>
        /// <param name="room"></param>
        public bool ChangeRoom(Room room)
        {
            if (this == ServerPlayer)
            {
                Debug.LogError("Tried to change ServerPlayer's room");
                return false;
            }
            if (room == null)
                throw new ArgumentNullException("room");

            var guid = room.Guid;
            if (!Server.TryGetRoom(guid, out room))
            {
                return false;
            }

            SendRoomSwitch(room);
            return true;
        }

        private Guid _switchToken;
        void SendRoomSwitch(Room room)
        {
            _switchToken = Guid.NewGuid();

            _switchingToRoom = room.Guid;

            //todo: tell old room that the player is going to leave.
            NetMessage rmsg;
            _oldRoom = _currentRoom;
            _currentRoom = Guid.Empty;

            if (Server.TryGetRoom(_oldRoom, out var oldRoom))
            {
                rmsg = Server.GetMessage(4);
                rmsg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
                rmsg.Write(DandRRpcs.ExpectLeavingPlayer);
                rmsg.Write(Id);
                oldRoom.SendMessage(rmsg, ReliabilityMode.Ordered);
            }

            
            var pmsg = Server.GetMessage(30 + room.RoomId.Length * 2);
            pmsg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            pmsg.Write(DandPRpcs.RoomSwitch);
            pmsg.Write(room.Address);  //this is ~4 bytes + 3 bytes
            pmsg.Write(room.RoomId);
            pmsg.Write(_switchToken);
            SendMessage(pmsg, ReliabilityMode.Ordered);
        }

        void ClientFinishedRoomSwitch()
        {
            if (_switchingToRoom == Guid.Empty)
            {
                Debug.LogError($"{this} said they had finished room switch, but they aren't currently in a room switching state");
                return;
            }

            if (!Server.TryGetRoom(_switchingToRoom, out var room))
            {
                Debug.LogError($"Could not get room {_switchingToRoom} when client notified us they were finishing switching their rooms. This is incomplete, and we should probably switch the player to a different room");
                return;
            }

            var rmsg = Server.GetMessage(20);
            rmsg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            rmsg.Write(DandRRpcs.ExpectPlayer);
            rmsg.Write(_switchToken);
            rmsg.Write(Id);
            room.SendMessage(rmsg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// called by rooms when a player finishes connecting
        /// </summary>
        internal void FinishedRoomSwitch()
        {
#if DEBUG
            Debug.Log($"Finished switching {this}");
#endif
            //update our actual room
            _currentRoom = _switchingToRoom;
            _switchingToRoom = Guid.Empty;
            Room.MovePlayerCount(Server, _oldRoom, _currentRoom);
            _oldRoom = Guid.Empty;
            //and synchronize data
            SynchNetData();
            FinishedSwitchingRooms?.Invoke(Server.GetRoom(_currentRoom));
        }

        /// <summary>
        /// disconnect the player with the specified reason sent to the player
        /// </summary>
        /// <param name="reason"></param>
        public void Disconnect(string reason)
        {
            if (this == ServerPlayer) return;

            ImplementationDisconnect(reason);

            DisconnectOnRoom();
        }
        partial void ImplementationDisconnect(string reason);

        /// <summary>
        /// tell the room, if this is in one, to disconnect the player
        /// </summary>
        internal void DisconnectOnRoom()
        {
            //tell the room to disconnect the player
            var room = Room;
            if (room == null) return;

            var rmsg = Server.GetMessage(4);
            rmsg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            rmsg.Write(DandRRpcs.DisconnectPlayer);
            rmsg.Write(Id);
            room.SendMessage(rmsg, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// Call this to allow the player to connect
        /// </summary>
        public void AllowConnect()
        {
            if (this == ServerPlayer) return;

            Server.BeginPlayerAdd(this);
            ImplementationAllowConnect();
        }
        partial void ImplementationAllowConnect();

        private void CallRpc(byte rpcId, NetMessage msg, PlayerMessageInfo info)
        {
            var proc = _rpcProcessors[rpcId];
            if (proc == null)
            {
                info.ContinueForwarding = false;
                Debug.LogWarning($"Unhandled player rpc {rpcId}");
            }
            else
                proc(msg, info);
        }

        #region IInfoRpcProvider<PlayerMessageInfo>
        readonly Action<NetMessage, PlayerMessageInfo>[] _rpcProcessors = new Action<NetMessage, PlayerMessageInfo>[256];

        /// <summary>
        /// call action whenever the rpcId is received
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="action"></param>
        public bool SubscribeToRpc(byte rpcId, Action<NetMessage, PlayerMessageInfo> action, bool overwriteExisting = true, bool defaultContinueForwarding = true)
        {
            _rpcProcessors[rpcId] = action;
            return true;
        }

        /// <summary>
        /// remove whichever action was subscribed to the specified rpcId
        /// </summary>
        /// <param name="rpcId"></param>
        public void UnsubscribeRpc(byte rpcId)
        {
            _rpcProcessors[rpcId] = null;
        }

        public void SubscribeRpcsOnObject(object obj)
        {
            RpcSubscriber.SubscribeObject<PlayerMessageInfo, RpcAttribute>(this, obj, Server.Serializer, Debug.Logger);
        }

        public void ClearSubscriptions()
        {
            for (int i = 0; i < _rpcProcessors.Length; i++)
            {
                _rpcProcessors[i] = null;
            }
        }
        #endregion
        
        #region IProxySingle<IPlayerProxy>
        private IPlayerProxy _proxyObject;

        /// <summary>
        /// the value set from Proxy(IRoomProxy proxy)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Proxy<T>()
        {
            return (T)_proxyObject;
        }
        /// <summary>
        /// set the proxy object to use when returning Proxy`T().
        /// This is only useful if you're using something like Castle.Windsor's dynamic proxy generation.
        /// </summary>
        /// <param name="proxy"></param>
        public void Proxy(IPlayerProxy proxy)
        {
            _proxyObject = proxy;
            if (proxy != null)
                proxy.Player = this;
        }
        #endregion

        /// <summary>
        /// Send a message created from the Server.SerializeRpc. NetMessage.Recycle MUST be called after this, as the messages are not recycled
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(NetMessage msg)
        {
            if (this == ServerPlayer) return;

            ImplSend(msg, ReliabilityMode.Ordered, false);
        }

        internal void SendMessage(NetMessage msg, ReliabilityMode mode)
        {
            if (this == ServerPlayer) return;

            ImplSend(msg, mode);
        }
        partial void ImplSend(NetMessage msg, ReliabilityMode mode, bool recycle = true);

        public override string ToString()
        {
            return string.Format("{{Player {0} {1} {2}}}", Id, UserData, NetUserData);
        }
    }
}