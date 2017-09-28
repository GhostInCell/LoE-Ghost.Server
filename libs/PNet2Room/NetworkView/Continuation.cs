using PNet;
using System;

namespace PNetR
{
    public abstract class AContinuation
    {
        internal readonly byte ComponentId;
        internal readonly byte RpcId;

        //make this class not extensible nor instancable outside of the dll
        private AContinuation() { }

        internal AContinuation(byte compId, byte rpcId)
        {
            ComponentId = compId;
            RpcId = rpcId;
        }

        internal void RunError(NetMessage msg, NetMessageInfo info)
        {
            try
            {
                OnError(msg.ReadString(), info);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void RunSuccess(NetMessage msg, NetMessageInfo info)
        {
            try
            {
                OnSuccess(msg, info);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        protected abstract void OnError(string msg, NetMessageInfo info);
        protected abstract void OnSuccess(NetMessage msg, NetMessageInfo info);
    }

    /// <summary>
    /// Represents a set of actions that will run when the server completes the called rpc
    /// </summary>
    /// <typeparam name="T">The return type from the server. Must have defined serialize/deserialize information on the server/client</typeparam>
    public class Continuation<T> : AContinuation
    {
        private readonly Func<NetMessage, T> _deserialize;
        private Action<T, NetMessageInfo> _complete;
        private Action<string, NetMessageInfo> _error;
        private Action<T, NetMessageInfo> _success;

        public Continuation() : this(0, 0, message => default(T)){}

        internal Continuation(byte compId, byte rpcId, Func<NetMessage, T> deserialize)
            : base(compId, rpcId)
        {
            _deserialize = deserialize;
        }

        /// <summary>
        /// Perform the action after either a success or an error
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Continuation<T> Complete(Action<T, NetMessageInfo> action)
        {
            _complete = action;
            return this;
        }

        /// <summary>
        /// Perform the action if the rpc call was successful
        /// </summary>
        /// <param name="action">the method to perform, with the supplied returned value</param>
        /// <returns></returns>
        public Continuation<T> Success(Action<T, NetMessageInfo> action)
        {
            _success = action;
            return this;
        }

        /// <summary>
        /// Perform the action if the rpc call had an error
        /// </summary>
        /// <param name="action">the method to perform, with a string describing the error</param>
        /// <returns></returns>
        public Continuation<T> Error(Action<string, NetMessageInfo> action)
        {
            _error = action;
            return this;
        }

        protected override void OnError(string msg, NetMessageInfo info)
        {
            _error?.Invoke(msg, info);
            _complete?.Invoke(default(T), info);
        }

        protected override void OnSuccess(NetMessage msg, NetMessageInfo info)
        {
            var val = _deserialize(msg);
            _success?.Invoke(val, info);
            _complete?.Invoke(val, info);
        }
    }

    public class Continuation : Continuation<NetMessage>
    {
        public Continuation() : this(0, 0){}
        internal Continuation(byte compId, byte rpcId) : base(compId, rpcId, msg => msg)
        {
        }
    }
}