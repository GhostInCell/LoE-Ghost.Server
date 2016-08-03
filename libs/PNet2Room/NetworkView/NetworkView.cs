using System;
using System.Collections.Generic;
using PNet;
using System.Linq;
using System.Numerics;

namespace PNetR
{
    public partial class NetworkView : IComponentInfoRpcProvider<NetMessageInfo>, IProxyCollection<INetComponentProxy>
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly NetworkViewManager Manager;
        private Player _owner;
        public readonly ushort Id;
        
        internal NetworkView(NetworkViewManager networkViewManager, ushort networkId, Player owner)
        {
            Manager = networkViewManager;
            Id = networkId;
            Owner = owner;
        }

        public Player Owner
        {
            get { return _owner; }
            internal set
            {
                _owner = value;
                ImplOwnerChanged();
            }
        }

        partial void ImplOwnerChanged();

        public string Resource { get; internal set; }
        public Room Room { get { return Manager.Room; } }

        public event Action<Player> FinishedInstantiation;
        public event Action<NetMessage, Player> ReceivedStream;
        public event Func<Vector3> GettingPosition;
        public event Func<Vector3> GettingRotation;

        public event Func<Player, bool> CheckVisibility;

        private readonly HashSet<Player> _visibility = new HashSet<Player>();

        internal bool OnPlayerEnteredRoom(Player player)
        {
            //we already have the player added to the connections list, as it was set when we set the Owner.
            if (Owner == player)
                return true;
          
            var vis = CheckVisibility?.Invoke(player) ?? true;
            if (vis)
            {
                ImplObservePlayer(player);
                _visibility.Add(player);
            }
            return vis;
        }

        partial void ImplObservePlayer(Player player);

        internal void OnPlayerLeftRoom(Player player)
        {
            _visibility.Remove(player);
            ImplIgnorePlayer(player);
        }

        partial void ImplIgnorePlayer(Player player);

        public void RebuildVisibility()
        {
            var pos = GettingPosition?.Invoke() ?? Vector3.Zero;
            var rot = GettingRotation?.Invoke() ?? Vector3.Zero;

            var msg = Room.ConstructInstMessage(this, pos, rot);
            RebuildVisibility(msg);
        }

        internal void RebuildVisibility(NetMessage ccMsg)
        {
            var destMsg = Room.GetDestroyMessage(this, RandPRpcs.Hide);
            var hidePlayers = new List<Player>();
            var showPlayers = new List<Player>();
            
            foreach (var player in Room.Players)
            {
                if (player == null) continue;
                if (!player.IsValid) continue;
                if (player == Owner) continue;

                var vis = CheckVisibility?.Invoke(player) ?? true;
                if (vis && !_visibility.Contains(player))
                {
                    ImplObservePlayer(player);
                    _visibility.Add(player);
                    showPlayers.Add(player);
                }
                else if (!vis && _visibility.Contains(player))
                {
                    _visibility.Remove(player);
                    ImplIgnorePlayer(player);
                    hidePlayers.Add(player);
                }
            }
            Room.SendToPlayers(hidePlayers.ToArray(), destMsg, ReliabilityMode.Ordered);
            Room.SendToPlayers(showPlayers.ToArray(), ccMsg, ReliabilityMode.Ordered);
        }

        internal void OnFinishedInstantiate(Player player)
        {
            //get the player up to speed
            SendBuffer(player);
            try
            {
                FinishedInstantiation?.Invoke(player);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SendBuffer(Player player)
        {
            //todo
        }

        #region IProxyCollection<INetComponentProxy>
        private readonly Dictionary<Type, INetComponentProxy> _proxies = new Dictionary<Type, INetComponentProxy>();
        /// <summary>
        /// the value set from Proxy(IRoomProxy proxy)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Proxy<T>()
        {
            INetComponentProxy proxy;
            if (_proxies.TryGetValue(typeof (T), out proxy))
            {
                var ret = (T) proxy;
                proxy.CurrentRpcMode = RpcMode.AllOrdered;
                proxy.CurrentSendTo = null;
                return ret;
            }
            return default(T);
        }

        /// <summary>
        /// the value set from Proxy(INetComponentProxy proxy)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Proxy<T>(RpcMode mode)
        {
            INetComponentProxy proxy;
            if (_proxies.TryGetValue(typeof(T), out proxy))
            {
                var ret = (T)proxy;
                proxy.CurrentRpcMode = mode;
                proxy.CurrentSendTo = null;
                return ret;
            }
            return default(T);
        }

        public T Proxy<T>(Player player)
        {
            INetComponentProxy proxy;
            if (_proxies.TryGetValue(typeof (T), out proxy))
            {
                var ret = (T) proxy;
                proxy.CurrentSendTo = player;
                proxy.CurrentRpcMode = RpcMode.ServerOrdered;
                return ret;
            }
            return default(T);
        }

        /// <summary>
        /// Add a proxy to the network view to use for Proxy`T()
        /// </summary>
        /// <param name="proxy"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddProxy(INetComponentProxy proxy)
        {
            if (proxy == null) 
                throw new ArgumentNullException("proxy");
            proxy.NetworkView = this;
            
            var ptype = proxy.GetType();
            var interfaces = ptype.GetInterfaces();
            _proxies[ptype] = proxy;
            foreach (var face in interfaces)
            {
                _proxies[face] = proxy;
            }
        }

        public void RemoveProxy(INetComponentProxy proxy)
        {
            if (proxy == null)
                throw new ArgumentNullException("proxy");

            var ptype = proxy.GetType();
            var interfaces = ptype.GetInterfaces();
            _proxies.Remove(ptype);
            foreach (var face in interfaces)
            {
                _proxies.Remove(face);
            }
        }

        public void RemoveProxy<T>()
        {
            var ptype = typeof (T);
            var faces = ptype.GetInterfaces();
            _proxies.Remove(ptype);
            foreach (var face in faces)
            {
                _proxies.Remove(face);
            }
        }

        public void ClearProxies()
        {
            _proxies.Clear();
        }

        #endregion

        #region IComponentInfoRpcProvider<NetMessageInfo>
        readonly Dictionary<int, RpcProcessor> _rpcProcessors = new Dictionary<int, RpcProcessor>();

        /// <summary>
        /// Subscribe to an rpc
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="rpcID">id of the rpc</param>
        /// <param name="rpcProcessor">action to process the rpc with</param>
        /// <param name="overwriteExisting">overwrite the existing processor if one exists.</param>
        /// <param name="defaultContinueForwarding">default value for info.continueForwarding</param>
        /// <returns>Whether or not the rpc was subscribed to. Will return false if an existing rpc was attempted to be subscribed to, and overwriteexisting was set to false</returns>
        public bool SubscribeToRpc(byte componentId, byte rpcID, Action<NetMessage, NetMessageInfo> rpcProcessor, bool overwriteExisting = true, bool defaultContinueForwarding = true)
        {
            if (rpcProcessor == null)
                throw new ArgumentNullException("rpcProcessor", "the processor delegate cannot be null");

            var id = (componentId << 8) | rpcID;

            if (overwriteExisting)
            {
                _rpcProcessors[id] = new RpcProcessor(rpcProcessor, defaultContinueForwarding);
                return true;
            }

            if (_rpcProcessors.ContainsKey(id))
            {
                return false;
            }

            _rpcProcessors.Add(id, new RpcProcessor(rpcProcessor, defaultContinueForwarding));
            return true;
        }

        /// <summary>
        /// subscribe a function to an rpc id. The return value of func will be sent back to the client, into the rpc that called it with RpcContinueWith
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="rpcId"></param>
        /// <param name="func"></param>
        /// <param name="overwriteExisting"></param>
        /// <returns></returns>
        public bool SubscribeToFunc(byte componentId, byte rpcId, Func<NetMessage, NetMessageInfo, object> func,
            bool overwriteExisting = true)
        {
            if (func == null)
                throw new ArgumentNullException("func", "the function delegate cannot be null");

            var id = (componentId << 8) | rpcId;

            if (overwriteExisting)
            {
                _rpcProcessors[id] = new RpcProcessor(func, false);
                return true;
            }
            if (_rpcProcessors.ContainsKey(id))
                return false;
            
            _rpcProcessors.Add(id, new RpcProcessor(func, false));
            return true;
        }

        /// <summary>
        /// Unsubscribe from an rpc
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="rpcID"></param>
        public void UnsubscribeFromRpc(byte componentId, byte rpcID)
        {
            var id = (componentId << 8) | rpcID;
            _rpcProcessors.Remove(id);
        }

        /// <summary>
        /// Unsubscribe all rpcs for component
        /// </summary>
        /// <param name="componentId"></param>
        public void UnsubscribeFromRpcs(byte componentId)
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                var id = (componentId << 8) | i;
                _rpcProcessors.Remove(id);
            }
        }

        /// <summary>
        /// Subscribe all the marked rpcs on the supplied component
        /// </summary>
        /// <param name="component"></param>
        public void SubscribeMarkedRpcsOnComponent(object component)
        {
            RpcSubscriber.SubscribeComponent<NetMessageInfo, RpcAttribute>(this, component, Room.Serializer, Debug.Logger);
        }

        public void ClearSubscriptions()
        {
            _rpcProcessors.Clear();
        }
        #endregion

        internal void IncomingStream(NetMessage msg, Player sender)
        {
            try
            {
                ReceivedStream?.Invoke(msg, sender);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void IncomingRpc(byte componentId, byte rpc, NetMessage msg, NetMessageInfo info, SubMsgType sub)
        {
            var id = (componentId << 8) | rpc;
            if (sub != SubMsgType.Out)
            {
                var cont = Dequeue(info.Sender.Id, componentId, rpc);
                if (cont == null) return;

                if (sub == SubMsgType.Reply)
                    cont.RunSuccess(msg, info);
                else if (sub == SubMsgType.Error)
                    cont.RunError(msg, info);
                return;
            }

            RpcProcessor processor;
            if (!_rpcProcessors.TryGetValue(id, out processor))
            {
                Debug.LogWarning($"Networkview on {componentId}: unhandled RPC {rpc}");
                info.ContinueForwarding = false;
            }
            else
            {
                info.ContinueForwarding = processor.DefaultContinueForwarding;
                if (processor.Action != null)
                {
                    try
                    {
                        processor.Action(msg, info);
                    }
                    catch (Exception e)
                    {
                        info.ContinueForwarding = false;
                        Debug.LogException(e);
                    }
                }
                else if (processor.Func != null)
                {
                    object ret;
                    try
                    {
                        ret = processor.Func(msg, info);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        ret = e;
                    }
                    //need to serialize and send back to the player.
                    ReturnFuncRpc(componentId, rpc, info.Sender, ret);
                }
                else
                {
                    //Debug.LogWarning("RPC processor for {0} was null. Automatically cleaning up. Please be sure to clean up after yourself in the future.", rpcID);
                    _rpcProcessors.Remove(id);
                }
            }
        }

        private void ReturnFuncRpc(byte componentId, byte rpc, Player sender, object ret)
        {
            var msg = Room.RoomGetMessage(5 + Room.Serializer.SizeOf(ret));
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Netview,
                ret is Exception ? SubMsgType.Error : SubMsgType.Reply));
            msg.Write(Id);
            msg.Write(componentId);
            msg.Write(rpc);
            Room.Serializer.Serialize(ret is Exception ? (ret as Exception).Message : ret, msg);
            SendMessage(msg, sender, ReliabilityMode.Ordered);
        }

        /// <summary>
        /// Create a stream to serialize into
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public NetMessage CreateStream(int size)
        {
            var msg = Room.RoomGetMessage(size + 3);
            msg.Write(RpcUtils.GetHeader(ReliabilityMode.Unreliable, BroadcastMode.All, MsgType.Stream));
            msg.Write(Id);
            return msg;
        }

        /// <summary>
        /// Send a message stream from CreateStream to all players
        /// </summary>
        /// <param name="msg"></param>
        public void SendStream(NetMessage msg)
        {
            ImplSendMessage(msg, ReliabilityMode.Unreliable, BroadcastMode.All);
        }

        /// <summary>
        /// Send a message stream from CreateStream to the specified players
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="players"></param>
        public void SendStream(NetMessage msg, List<Player> players)
        {
            ImplSendMessage(msg, players, ReliabilityMode.Unreliable);
        }

        internal Vector3 GetPosition()
        {
            return GettingPosition?.Invoke() ?? Vector3.Zero;
        }

        internal Vector3 GetRotation()
        {
            return GettingRotation?.Invoke() ?? Vector3.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        public event Action Destroyed;
        internal void Destroy()
        {
            try
            {
                Destroyed?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            Destroyed = null;
        }

        public override string ToString()
        {
            return "NV " + Id;
        }
    }
}