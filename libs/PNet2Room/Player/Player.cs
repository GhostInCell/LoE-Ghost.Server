using PNet;
using System;
using System.Net;

namespace PNetR
{
    public partial class Player
    {
        public ushort Id { get; internal set; }
        /// <summary>
        /// token generated during connection/room switching
        /// </summary>
        public Guid Token { get; internal set; }

        /// <summary>
        /// the actual endpoint connection to the player
        /// </summary>
        public IPEndPoint EndPoint { get; internal set; }

        public object Connection;

        internal readonly Room Room;

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

        internal Player(Room room)
        {
            Room = room;
        }

        internal Player(Room room, INetSerializable netUserData)
        {
            Room = room;
            NetUserData = netUserData;
        }

        private int _internalErrorCount;

        /// <summary>
        /// number of times a player can incur internal errors before being automatically disconnected
        /// </summary>
        public static int MaxInternalErrorCount = 100;

        /// <summary>
        /// custom object to associate with the player. not synched over the network.
        /// </summary>
        public object UserData;

        /// <summary>
        /// Custom object to associate with the player, synchronizable over the network.
        /// </summary>
        public readonly INetSerializable NetUserData;
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
        /// fired when the NetUserData deserializes from an update over the network
        /// </summary>
        public event Action<Player> NetUserDataChanged;

        internal virtual void OnNetUserDataChanged()
        {
#if DEBUG
            Debug.Log($"{this} data changed");
#endif
            NetUserDataChanged?.Invoke(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SynchNetData()
        {
            if (NetUserData == null) return false;
            var msg = Room.ServerGetMessage(NetUserData.AllocSize + 4);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            msg.Write(DandRRpcs.SyncNetUser);
            msg.Write(Id);
            NetUserData.OnSerialize(msg);
            Room.Server.SendMessage(msg, ReliabilityMode.Ordered);
            return true;
        }

        /// <summary>
        /// change the player to the specified room
        /// </summary>
        /// <param name="roomName"></param>
        public void ChangeRoom(string roomName)
        {
            var msg = Room.ServerGetMessage(roomName.Length * 2 + 2);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            msg.Write(DandRRpcs.RoomSwitch);
            msg.Write(Id);
            msg.Write(roomName);
            Room.Server.SendMessage(msg, ReliabilityMode.Ordered);
        }

        public void ChangeRoom(Guid roomId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// disconnect the player with the specified reason sent to the player
        /// </summary>
        /// <param name="reason"></param>
        public void Disconnect(string reason)
        {
            //todo: send disconnect message to PNetS
            Room.Disconnect(this, reason);
        }

        internal void AllowConnect()
        {
            Room.AllowConnect(this);
        }

        internal void SendMessage(NetMessage msg, ReliabilityMode mode)
        {
            Room.SendToPlayer(this, msg, mode);
        }

        private static Player _serverPlayer;
        public static Player Server
        {
            get { return _serverPlayer ?? (_serverPlayer = new Player(null){Id = 0}); }
        }

        public bool IsValid => Id != 0;

        private IPlayerProxy _proxyObject;

        /// <summary>
        /// the value set from Proxy(IRoomProxy proxy)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Proxy<T>() => (T)_proxyObject;

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

        public override string ToString() => $"{{Player {Id} {UserData} {NetUserData}}}";

        /// <summary>
        /// Create an invalid "player" with almost identical user data.
        /// </summary>
        /// <returns></returns>
        internal Player CopyInvalid()
        {
            var inv = new Player(Room, NetUserData)
            {
                UserData = UserData,
                _proxyObject = _proxyObject,
                _internalErrorCount = _internalErrorCount
            };
            return inv;
        }
    }
}
