using PNet;

namespace PNetR
{
    public interface INetComponentProxy
    {
        /// <summary>
        /// The current thread's rpc mode.
        /// </summary>
        RpcMode CurrentRpcMode { get; set; }
        Player CurrentSendTo { get; set; }
        NetworkView NetworkView { get; set; }
    }
}