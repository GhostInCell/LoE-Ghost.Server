using PNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PNetS
{
    public partial class Room : IRpcProvider, IProxySingle<IRoomProxy>
    {
        public readonly Guid Guid;
        public readonly string RoomId;
        public readonly IPEndPoint Address;
        private int _playerCount;

        public bool Running = true;

        private readonly Server _server;

        public object Connection { get; set; }

        internal Room(Server server, string roomId, Guid guid, IPEndPoint address)
        {
            Guid = guid;
            _server = server;
            RoomId = roomId;
            Address = address;
        }

        /// <summary>
        /// maximum players the room supports, sent to the server by the room.
        /// </summary>
        public int MaxPlayers { get; internal set; }

        /// <summary>
        /// Number of players in the room.
        /// </summary>
        public int PlayerCount
        {
            get { return _playerCount; }
        }

        public IEnumerable<Player> Players
        {
            get
            {
                Player[] players;
                lock (_server.Players)
                {
                    //values is a snapshot, so we don't need to lock on it for the linq.
                    players = _server.Players.Values;
                }
                return players.Where(player => player != null && player.CurrentRoomGuid == Guid);
            }
        }

        static readonly object CountLock = new object();
        /// <summary>
        /// move the a player count for the specified server from oldRoom to newRoom
        /// </summary>
        /// <param name="server"></param>
        /// <param name="oldRoom"></param>
        /// <param name="newRoom"></param>
        internal static void MovePlayerCount(Server server, Guid oldRoom, Guid newRoom)
        {
            if (server == null)
                throw new NullReferenceException("Server is null");

            var old = server.GetRoom(oldRoom);
            var @new = server.GetRoom(newRoom);

            try
            {
                lock (CountLock)
                {
                    if (old != null)
                        old._playerCount--;
                    if (@new != null)
                        @new._playerCount++;
                }
            }
            catch (NullReferenceException ex)
            {
                throw new Exception($"MovePlayerCount nullref: {old}|{@new}|{server}");
            }
        }

        private void CallRpc(byte rpcId, NetMessage msg)
        {
            var proc = _rpcProcessors[rpcId];
            if (proc == null)
                Debug.LogWarning($"Unhandled room rpc {rpcId}");
            else
                proc(msg);
        }

        #region IRpcProvider
        readonly Action<NetMessage>[] _rpcProcessors = new Action<NetMessage>[256];

        public bool SubscribeToRpc(byte rpcId, Action<NetMessage> action)
        {
            _rpcProcessors[rpcId] = action;
            return true;
        }

        public void UnsubscribeRpc(byte rpcId)
        {
            _rpcProcessors[rpcId] = null;
        }

        public void SubscribeRpcsOnObject(object obj)
        {
            RpcSubscriber.SubscribeObject<RpcAttribute>(this, obj, _server.Serializer, Debug.Logger);
        }

        public void ClearSubscriptions()
        {
            for (int i = 0; i < _rpcProcessors.Length; i++)
            {
                _rpcProcessors[i] = null;
            }
        }
        #endregion

        internal void SendMessage(NetMessage msg, ReliabilityMode mode)
        {
            _server.SendToRoom(this, msg, mode);
        }

        internal void SendMessageToOthers(NetMessage msg, ReliabilityMode mode)
        {
            _server.SendToOtherRooms(this, msg, mode);
        }

        internal void SendToAll(NetMessage msg, ReliabilityMode mode)
        {
            _server.SendToAllRooms(msg, mode);
        }

        private IRoomProxy _proxyObject;

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
        public void Proxy(IRoomProxy proxy)
        {
            _proxyObject = proxy;
            if (proxy != null)
                proxy.Room = this;
        }

        public override string ToString() => $"{{Room {Guid}:{RoomId}}}";

        /// <summary>
        /// User defined auth data passed during start
        /// </summary>
        public string UserDefinedAuthData { get; internal set; }
    }
}
