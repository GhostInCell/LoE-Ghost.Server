using System;
using System.Collections.Generic;
using PNet;

namespace PNetR
{
    /// <summary>
    /// Network view, but for scene objects
    /// </summary>
    public class NetworkedSceneObjectView: IInfoRpcProvider<NetMessageInfo>, IProxySingle<ISceneViewProxy>
    {
        internal NetworkedSceneObjectView(SceneViewManager sceneViewManager, Room room)
        {
            Manager = sceneViewManager;
            _room = room;
        }
        readonly Room _room;

        /// <summary>
        /// 
        /// </summary>
        public readonly SceneViewManager Manager;

        /// <summary>
        /// The scene/room Network ID of this item. Should only be one per room
        /// </summary>
        public ushort NetworkID { get; internal set; }

        #region IInfoRpcProvider<NetMessageInfo>

        readonly Dictionary<byte, RpcProcessor> _rpcProcessors = new Dictionary<byte, RpcProcessor>();

        /// <summary>
        /// Subscribe to an rpc
        /// </summary>
        /// <param name="rpcID">id of the rpc</param>
        /// <param name="rpcProcessor">action to process the rpc with</param>
        /// <param name="overwriteExisting">overwrite the existing processor if one exists.</param>
        /// <param name="defaultContinueForwarding">default info.continueForwarding value</param>
        /// <returns>Whether or not the rpc was subscribed to. Will return false if an existing rpc was attempted to be subscribed to, and overwriteexisting was set to false</returns>
        public bool SubscribeToRpc(byte rpcID, Action<NetMessage, NetMessageInfo> rpcProcessor, bool overwriteExisting = true, bool defaultContinueForwarding = true)
        {
            if (rpcProcessor == null)
                throw new ArgumentNullException("rpcProcessor", "the processor delegate cannot be null");
            if (overwriteExisting)
            {
                _rpcProcessors[rpcID] = new RpcProcessor(rpcProcessor, defaultContinueForwarding);
                return true;
            }
            else
            {
                if (_rpcProcessors.ContainsKey(rpcID))
                {
                    return false;
                }
                else
                {
                    _rpcProcessors.Add(rpcID, new RpcProcessor(rpcProcessor, defaultContinueForwarding));
                    return true;
                }
            }
        }

        /// <summary>
        /// Unsubscribe from an rpc
        /// </summary>
        /// <param name="rpcID"></param>
        public void UnsubscribeRpc(byte rpcID)
        {
            _rpcProcessors.Remove(rpcID);
        }

        internal void CallRpc(byte rpcID, NetMessage message, NetMessageInfo info)
        {
            if (_rpcProcessors.TryGetValue(rpcID, out var processor))
            {
                info.ContinueForwarding = processor.DefaultContinueForwarding;

                if (processor.Action != null)
                    processor.Action(message, info);
                else
                {
                    Debug.LogWarning($"RPC processor for {rpcID} was null. Automatically cleaning up. Please be sure to clean up after yourself in the future.");
                    _rpcProcessors.Remove(rpcID);
                }
            }
            else if (!Manager.AllowUnhandledRpcForwarding)
            {
                Debug.LogWarning($"NetworkedSceneView {rpcID} received unhandled RPC {NetworkID}");
                info.ContinueForwarding = false;
            }
        }

        public void SubscribeRpcsOnObject(object obj)
        {
            RpcSubscriber.SubscribeObject<NetMessageInfo, RpcAttribute>(this, obj, _room.Serializer, Debug.Logger);
        }

        public void ClearSubscriptions()
        {
            _rpcProcessors.Clear();
        }

        /// <summary>
        /// Unsubscribe all marked rpcs on listener
        /// </summary>
        /// <param name="listener"></param>
        public void Unsubscribe(object listener)
        {
            var cType = listener.GetType();

            if (cType == typeof(NetworkView)) //speedup
                return;

            RpcSubscriber.ForEachRpc<RpcAttribute>(cType, (method, parms, parmTypes, tokens) =>
            {
                foreach (var token in tokens)
                {
                    UnsubscribeRpc(token.Item2.RpcId);
                }
            });
        }
        #endregion

        /// <summary>
        /// Send an rpc to all in the room.
        /// </summary>
        /// <param name="rpcID"></param>
        /// <param name="args"></param>
        public void Rpc(byte rpcID, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += _room.Serializer.SizeOf(arg);
            }

            var msg = StartMessage(rpcID, RpcMode.AllOrdered, size);
            foreach (var arg in args)
            {
                _room.Serializer.Serialize(arg, msg);
            }
            _room.SendSceneView(msg, ReliabilityMode.Ordered);
        }

        NetMessage StartMessage(byte rpcId, RpcMode mode, int size)
        {
            var msg = _room.RoomGetMessage(size + 5);

            msg.Write(RpcUtils.GetHeader(mode, MsgType.Internal));
            msg.Write(RandPRpcs.SceneObjectRpc);
            msg.Write(NetworkID);
            msg.Write(rpcId);
            return msg;
        }

        #region IProxySingle<ISceneViewProxy>
        private ISceneViewProxy _proxyObject;
        /// <summary>
        /// the value set from Proxy(IServerProxy proxy)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Proxy<T>()
        {
            return (T)_proxyObject;
        }
        /// <summary>
        /// set the proxy object to use when returning Proxy`T()
        /// </summary>
        /// <param name="proxy"></param>
        public void Proxy(ISceneViewProxy proxy)
        {
            _proxyObject = proxy;
            if (_proxyObject != null)
                proxy.View = this;
        }
        #endregion
    }
}
