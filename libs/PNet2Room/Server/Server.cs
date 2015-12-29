using System;
using System.Collections.Generic;
using PNet;

namespace PNetR
{
    public partial class Server : IRpcProvider, IProxySingle<IServerProxy>
    {
        private readonly Room _room;
        internal Server(Room room)
        {
            _room = room;
        }

        internal void SendMessage(NetMessage msg, ReliabilityMode mode)
        {
            ImplementationSendMessage(msg, mode);
        }
        partial void ImplementationSendMessage(NetMessage msg, ReliabilityMode mode);

        private void CallRpc(byte rpcId, NetMessage msg)
        {
            var proc = _rpcProcessors[rpcId];
            if (proc == null)
            {
                Debug.LogWarning($"Unhandled server rpc {rpcId}");
            }
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
            RpcSubscriber.SubscribeObject<RpcAttribute>(this, obj, _room.Serializer, Debug.Logger);
        }

        public void ClearSubscriptions()
        {
            for (int i = 0; i < _rpcProcessors.Length; i++)
            {
                _rpcProcessors[i] = null;
            }
        }
        #endregion

        private readonly Dictionary<Guid, string> _rooms = new Dictionary<Guid, string>();
        private readonly Dictionary<string, int> _roomNames = new Dictionary<string, int>();

        public bool HasRoom(string roomName)
        {
            return _roomNames.ContainsKey(roomName);
        }

        private IServerProxy _proxyObject;
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
        public void Proxy(IServerProxy proxy)
        {
            _proxyObject = proxy;
            if (proxy != null)
                proxy.Server = this;
        }
    }
}