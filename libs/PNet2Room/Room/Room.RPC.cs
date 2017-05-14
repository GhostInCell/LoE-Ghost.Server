using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PNet;
using SharpFactory.DMDDemo;

namespace PNetR
{
    partial class Room : IInfoRpcProvider<NetMessageInfo>
    {
        readonly Dictionary<int, RpcProcessor> _rpcProcessors = new Dictionary<int, RpcProcessor>();

        #region IInfoRpcProvider<NetMessageInfo>
        /// <summary>
        /// Subscribe to an rpc
        /// </summary>
        /// <param name="rpcID">id of the rpc</param>
        /// <param name="rpcProcessor">action to process the rpc with</param>
        /// <param name="overwriteExisting">overwrite the existing processor if one exists.</param>
        /// <param name="defaultContinueForwarding">default value for info.continueForwarding</param>
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

            if (_rpcProcessors.ContainsKey(rpcID))
            {
                return false;
            }

            _rpcProcessors.Add(rpcID, new RpcProcessor(rpcProcessor, defaultContinueForwarding));
            return true;
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
                    UnsubscribeRpc(token.Value.RpcId);
                }
            });
        }

        /// <summary>
        /// unsubscribe the specified rpc
        /// </summary>
        /// <param name="rpcId"></param>
        public void UnsubscribeRpc(byte rpcId)
        {
            _rpcProcessors.Remove(rpcId);
        }

        /// <summary>
        /// Subscribe all the marked rpcs on the supplied object
        /// </summary>
        /// <param name="obj"></param>
        public void SubscribeRpcsOnObject(object obj)
        {
            RpcSubscriber.SubscribeObject<NetMessageInfo, RpcAttribute>(this, obj, Serializer, Debug.Logger);
        }

        /// <summary>
        /// Clear all rpc subscriptions
        /// </summary>
        public void ClearSubscriptions()
        {
            _rpcProcessors.Clear();
        }
        #endregion

        internal void CallRpc(NetMessage msg, NetMessageInfo info)
        {
            if (!msg.ReadByte(out var rpc))
            {
                Debug.LogError("Malformed static rpc - no rpc id");
                return;
            }

            if (!_rpcProcessors.TryGetValue(rpc, out var processor))
            {
                Debug.LogWarning($"Unhandled player RPC {rpc}");
                info.ContinueForwarding = false;
            }
            else
            {
                info.ContinueForwarding = processor.DefaultContinueForwarding;
                if (processor.Action != null)
                    processor.Action(msg, info);
                else
                {
                    //Debug.LogWarning("RPC processor for {0} was null. Automatically cleaning up. Please be sure to clean up after yourself in the future.", rpcID);
                    _rpcProcessors.Remove(rpc);
                }
            }
        }
    }
}
