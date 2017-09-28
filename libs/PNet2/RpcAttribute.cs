using System;

namespace PNet
{
    /// <summary>
    /// Attribute for marking rpc methods
    /// </summary>
    /// <remarks>
    /// Only one rpc attribute is valid per rpc id per receiving object (room, networkview, etc). If there are multiple, they are overwritten
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RpcAttribute : Attribute, IRpcAttribute
    {
        /// <summary>
        /// id of the rpc
        /// </summary>
        public byte RpcId { get; private set; }

        /// <summary>
        /// Server only. what the default value for continue forwarding is set to
        /// </summary>
        public bool DefaultContinueForwarding { get; private set; }

        /// <summary>
        /// mark the specified method with this rpc id
        /// </summary>
        /// <param name="rpcId"></param>
        public RpcAttribute(byte rpcId)
        {
            RpcId = rpcId;
            DefaultContinueForwarding = true;
        }

        /// <summary>
        /// Server only
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="defaultContinueForwarding">what the default value for continue forwarding is set to</param>
        public RpcAttribute(byte rpcId, bool defaultContinueForwarding)
        {
            RpcId = rpcId;
            DefaultContinueForwarding = defaultContinueForwarding;
        }
    }

    public interface IRpcAttribute
    {
        byte RpcId { get; }
        bool DefaultContinueForwarding { get; }
    }
}
