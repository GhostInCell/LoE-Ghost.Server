using System;
using PNet;

namespace PNetR
{
    internal struct RpcProcessor
    {
        public readonly Action<NetMessage, NetMessageInfo> Action;
        public readonly Func<NetMessage, NetMessageInfo, object> Func;
        public readonly bool DefaultContinueForwarding;

        public RpcProcessor(Action<NetMessage, NetMessageInfo> action, bool defaultContinueForwarding)
        {
            Action = action;
            DefaultContinueForwarding = defaultContinueForwarding;
            Func = null;
        }

        public RpcProcessor(Func<NetMessage, NetMessageInfo, object> func, bool defaultContinueForwarding)
        {
            Action = null;
            DefaultContinueForwarding = defaultContinueForwarding;
            Func = func;
        }
    }
}