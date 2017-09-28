using SharpFactory.DMDDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PNet
{
    public delegate object DynamicMethodDelegate(object target, object[] args);

    public static class RpcSubscriber
    {
        static readonly Dictionary<RuntimeMethodHandle, DynamicMethodDelegate> RpcCallers = new Dictionary<RuntimeMethodHandle, DynamicMethodDelegate>();

        public static void SetDynamicMethodDelegate(MethodInfo info, DynamicMethodDelegate @delegate)
        {
            RpcCallers[info.MethodHandle] = @delegate;
        }

        public static void SetDynamicMethodDelegate(RuntimeMethodHandle handle, DynamicMethodDelegate @delegate)
        {
            RpcCallers[handle] = @delegate;
        }

        public static DynamicMethodDelegate GetDynamicDelegate(MethodInfo info)
        {
            if (!RpcCallers.TryGetValue(info.MethodHandle, out var value)) return null;
            return value;
        }

        /// <summary>
        /// Subscribe TAttr marked methods on obj to the provider
        /// </summary>
        /// <typeparam name="TAttr">Attribute type</typeparam>
        /// <param name="provider"></param>
        /// <param name="obj"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public static void SubscribeObject<TAttr>(IRpcProvider provider, object obj, SerializationManager serializer, ILogger logger)
            where TAttr : Attribute, IRpcAttribute
        {
            if (obj == null) return;
            if (provider == obj) return;

            var objType = obj.GetType();
            //logger.Info("Subscribing " + obj);

            ForEachRpc<TAttr>(objType, (method, parms, parmTypes, tokens) =>
            {
                if (Delegate.CreateDelegate(typeof(Action<NetMessage>), obj, method, false) is Action<NetMessage> msgDel)
                    SubscribeTokens(provider, tokens, msgDel);
                else if (method.ReturnType == typeof(void))
                {
                    //the function isn't a deserializer function, so attempt to make our own from INetSerializable/default serializers
                    if (!CheckParameterSerialization<BadInfoType>(serializer, method, parms, logger))
                        return;

                    RpcCallers.TryGetValue(method.MethodHandle, out var pre);

                    //prevent the open delegate from needing a reference to this networkview?
                    var deser = new RpcDeserializer<BadInfoType>(method, obj, serializer, parmTypes, @delegate: pre);
                    msgDel = deser.Message;
                    SubscribeTokens(provider, tokens, msgDel);
                }
                else
                {
                    //method returns something, so it's a func processor
                    logger.Error("Cannot subscribe method with a return type other than void");
                }
            });
        }

        /// <summary>
        /// Subscribe TAttr marked methods on obj to the provider
        /// </summary>
        /// <typeparam name="TInfo"></typeparam>
        /// <typeparam name="TAttr"></typeparam>
        /// <param name="provider"></param>
        /// <param name="obj"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public static void SubscribeObject<TInfo, TAttr>(IInfoRpcProvider<TInfo> provider, object obj, SerializationManager serializer, ILogger logger)
            where TAttr : Attribute, IRpcAttribute
        {
            if (obj == null) return;
            if (provider == obj) return;

            var objType = obj.GetType();
            //logger.Info("Subscribing " + obj);

            ForEachRpc<TAttr>(objType, (method, parms, parmTypes, tokens) =>
            {
                if (Delegate.CreateDelegate(typeof(Action<NetMessage, TInfo>), obj, method, false) is Action<NetMessage, TInfo> msgDel)
                    SubscribeTokens(provider, tokens, msgDel);
                else if (method.ReturnType == typeof(void))
                {
                    //the function isn't a deserializer function, so attempt to make our own from INetSerializable/default serializers
                    if (!CheckParameterSerialization<TInfo>(serializer, method, parms, logger))
                        return;

                    RpcCallers.TryGetValue(method.MethodHandle, out var pre);

                    //prevent the open delegate from needing a reference to this networkview?
                    var deser = new RpcDeserializer<TInfo>(method, obj, serializer, parmTypes, @delegate: pre);
                    msgDel = deser.Message;
                    SubscribeTokens(provider, tokens, msgDel);
                }
                else
                {
                    //method returns something, so it's a func processor
                    logger.Error("Cannot subscribe method with a return type other than void");
                }
            });
        }

        /// <summary>
        /// Subscribe TAttr marked methods on obj to the provider
        /// </summary>
        /// <typeparam name="TInfo"></typeparam>
        /// <typeparam name="TAttr"></typeparam>
        /// <param name="provider"></param>
        /// <param name="obj"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public static void SubscribeComponent<TInfo, TAttr>(IComponentInfoRpcProvider<TInfo> provider, object obj,
            SerializationManager serializer, ILogger logger) where TAttr : Attribute, IRpcAttribute
        {
            if (obj == null) return;
            if (provider == obj) return;

            var objType = obj.GetType();

            //logger.Info("Subscribing " + obj);
            if (!objType.GetNetId(out var compId))
                throw new Exception("Cannot subscribe type " + objType + " as it lacks the NetComponentAttribute");

            ForEachRpc<TAttr>(objType, (method, parms, parmTypes, tokens) =>
            {
                if (Delegate.CreateDelegate(typeof(Action<NetMessage, TInfo>), obj, method, false) is Action<NetMessage, TInfo> msgDel)
                {
                    SubscribeTokens(provider, compId, tokens, msgDel);
                    return;
                }
                if (Delegate.CreateDelegate(typeof(Func<NetMessage, TInfo, object>), obj, method, false) is Func<NetMessage, TInfo, object> fncDel)
                {
                    SubscribeTokens(provider, compId, tokens, fncDel);
                    return;
                }

                if (method.ReturnType == typeof(void))
                {
                    //the function isn't a deserializer function, so attempt to make our own from INetSerializable/default serializers
                    if (!CheckParameterSerialization<TInfo>(serializer, method, parms, logger))
                        return;

                    var filterProviders = GetAttributes<IComponentRpcFilterProvider<TInfo>>(objType, method, parmTypes);
                    var filters = filterProviders.Select(fprov => fprov.Item2.GetFilter(provider)).ToArray();

                    RpcCallers.TryGetValue(method.MethodHandle, out var pre);
                    //prevent the open delegate from needing a reference to this networkview?
                    var deser = new RpcDeserializer<TInfo>(method, obj, serializer, parmTypes, filters, pre);
                    msgDel = deser.Message;
                    SubscribeTokens(provider, compId, tokens, msgDel);
                }
                else
                {
                    //method returns something, so it's a func processor
                    if (!CheckParameterSerialization<TInfo>(serializer, method, parms, logger))
                        return;
                    if (!serializer.CanSerialize(method.ReturnType))
                    {
                        logger.Error($"Tried to subscribe method {method} for rpc functions, but return type {method.ReturnType} cannot be serialized");
                        return;
                    }

                    RpcCallers.TryGetValue(method.MethodHandle, out var pre);
                    var deser = new RpcDeserializer<TInfo>(method, obj, serializer, parmTypes, @delegate: pre);
                    fncDel = deser.ReturnMessage;
                    SubscribeTokens(provider, compId, tokens, fncDel);
                }
            });
        }

        /// <summary>
        /// Subscribe TAttr marked methods on obj to the provider
        /// </summary>
        /// <typeparam name="TInfo"></typeparam>
        /// <typeparam name="TAttr"></typeparam>
        /// <param name="provider"></param>
        /// <param name="obj"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public static void SubscribeComponent<TAttr>(IComponentRpcProvider provider, object obj,
            SerializationManager serializer, ILogger logger) where TAttr : Attribute, IRpcAttribute
        {
            if (obj == null) return;
            if (provider == obj) return;

            var objType = obj.GetType();

            //logger.Info("Subscribing " + obj);
            if (!objType.GetNetId(out var compId))
                throw new Exception("Cannot subscribe type " + objType + " as it lacks the NetComponentAttribute");

            ForEachRpc<TAttr>(objType, (method, parms, parmTypes, tokens) =>
            {
                if (Delegate.CreateDelegate(typeof(Action<NetMessage>), obj, method, false) is Action<NetMessage> msgDel)
                {
                    SubscribeTokens(provider, compId, tokens, msgDel);
                    return;
                }
                if (Delegate.CreateDelegate(typeof(Func<NetMessage, object>), obj, method, false) is Func<NetMessage, object> fncDel)
                {
                    SubscribeTokens(provider, compId, tokens, fncDel);
                    return;
                }

                if (method.ReturnType == typeof(void))
                {
                    //the function isn't a deserializer function, so attempt to make our own from INetSerializable/default serializers
                    if (!CheckParameterSerialization<BadInfoType>(serializer, method, parms, logger))
                        return;

                    RpcCallers.TryGetValue(method.MethodHandle, out var pre);
                    //prevent the open delegate from needing a reference to this networkview?
                    var deser = new RpcDeserializer<BadInfoType>(method, obj, serializer, parmTypes, @delegate: pre);
                    msgDel = deser.Message;
                    SubscribeTokens(provider, compId, tokens, msgDel);
                }
                else
                {
                    //method returns something, so it's a func processor
                    if (!CheckParameterSerialization<BadInfoType>(serializer, method, parms, logger))
                        return;
                    if (!serializer.CanSerialize(method.ReturnType))
                    {
                        logger.Error($"Tried to subscribe method {method} for rpc functions, but return type {method.ReturnType} cannot be serialized");
                        return;
                    }

                    RpcCallers.TryGetValue(method.MethodHandle, out var pre);

                    var deser = new RpcDeserializer<object>(method, obj, serializer, parmTypes, @delegate: pre);
                    fncDel = deser.ReturnMessage;
                    SubscribeTokens(provider, compId, tokens, fncDel);
                }
            });
        }

        internal delegate void EachRpcAction<TAttr>(
            MethodInfo method, ParameterInfo[] parameters, Type[] paramTypes, List<(byte?, TAttr)> attributes);
        /// <summary>
        /// Execute the specified action on each method with the specified TAttr attribute, including methods that implement interfaces with the attribute
        /// </summary>
        /// <typeparam name="TAttr"></typeparam>
        /// <param name="objType"></param>
        /// <param name="action"></param>
        internal static void ForEachRpc<TAttr>(Type objType, EachRpcAction<TAttr> action) 
            where TAttr : Attribute
        {
            foreach (var method in GetMethods(objType))
            {
                GetParameters(method, out var parms, out var parmTypes);
                var tokens = GetAttributes<TAttr>(objType, method, parmTypes);
                if (tokens.Count <= 0) continue;
                action(method, parms, parmTypes, tokens);
            }
        }

        private static void GetParameters(MethodInfo method, out ParameterInfo[] parms, out Type[] parmTypes)
        {
            parms = method.GetParameters();
            parmTypes = parms.Select(p => p.ParameterType).ToArray();
        }

        private class BadInfoType
        {
            //as this is a private class, this ensures that lastIsInfo will always return false for non-info rpc deserialization
        }

        struct RpcDeserializer<T>
        {
            private readonly object _target;
            private readonly DynamicMethodDelegate _delegate;
            private readonly SerializationManager _serializer;
            private readonly Type[] _paramTypes;
            private readonly IRpcFilter<T>[] _filters;
            private readonly bool[] _parmOptionals;
            private readonly object[] _parmOptionDefaults;

            public RpcDeserializer(MethodInfo method, object target, SerializationManager serializer, Type[] paramTypes, 
                IRpcFilter<T>[] filters = null, DynamicMethodDelegate @delegate = null)
            {
                if (@delegate == null)
                    _delegate = DynamicMethodDelegateFactory.Create(method);
                else
                    _delegate = @delegate;

                _target = target;
                _serializer = serializer;
                _paramTypes = paramTypes;
                if (filters != null)
                    _filters = filters;
                else
                    _filters = new IRpcFilter<T>[0];

                var parms = method.GetParameters();
                _parmOptionals = new bool[parms.Length];
                _parmOptionDefaults = new object[parms.Length];
                for (int i = 0; i < parms.Length; i++)
                {
                    var isOptional = parms[i].IsOptional;
                    _parmOptionals[i] = isOptional;
                    if (isOptional)
                        _parmOptionDefaults[i] = parms[i].DefaultValue;
                }
            }

            public void Message(NetMessage message)
            {
                ReturnMessage(message, default(T));
            }

            public void Message(NetMessage message, T info)
            {
                foreach(var f in _filters)
                    if (!f.Filter(info)) return;
                ReturnMessage(message, info);
            }

            public object ReturnMessage(NetMessage message)
            {
                return ReturnMessage(message, default(T));
            }

            public object ReturnMessage(NetMessage message, T info)
            {
                var parms = new object[_paramTypes.Length];

                for (int i = 0; i < _paramTypes.Length; i++)
                {
                    object parm;
                    if (_paramTypes[i] == typeof (T))
                    {
                        parm = info;
                    }
                    else if (message.RemainingBits == 0 && _parmOptionals[i])
                    {
                        parm = _parmOptionDefaults[i];
                    }
                    else
                    {
                        parm = _serializer.Deserialize(_paramTypes[i], message);
                        if (parm == null) return null;
                    }
                    parms[i] = parm;
                }

                return _delegate(_target, parms);
            }
        }

        internal static bool CheckParameterSerialization<TInfo>(SerializationManager ser, MethodInfo method, ParameterInfo[] methodParameters, ILogger logger)
        {
            foreach (var parm in methodParameters)
            {
                if (parm.ParameterType == typeof (TInfo)) continue;
                if (ser.CanDeserialize(parm.ParameterType)) continue;

                logger.Warning($"Tried to subscribe method {method}.{parm} for rpc calls, but parameter {method.DeclaringType} cannot be deserialized");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get all attributes on the specified method with the specified type, including marked methods on implemented interfaces
        /// </summary>
        /// <typeparam name="TAttr"></typeparam>
        /// <param name="objType"></param>
        /// <param name="method"></param>
        /// <param name="parmTypes"></param>
        /// <returns></returns>
        public static List<(byte?, TAttr)> GetAttributes<TAttr>(Type objType, MethodInfo method, Type[] parmTypes) 
            where TAttr : class
        {
            var tokens = method.GetCustomAttributes(false).OfType<TAttr>().Select(x => (default(byte?), x)).ToList();
            FillAttributesFromInterfaces(objType, method, parmTypes, tokens);
            return tokens;
        }

// ReSharper disable once ReturnTypeCanBeEnumerable.Local
        static MethodInfo[] GetMethods(Type objType)
        {
            return objType.GetMethods(
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.FlattenHierarchy
                );
        }

        static void FillAttributesFromInterfaces<TAttr>(Type objType, MethodInfo method, Type[] parmTypes, List<(byte?, TAttr)> tokens) 
            where TAttr : class
        {
            foreach (var inter in objType.GetInterfaces())
            {
                var interMethod = inter.GetMethod(method.Name, parmTypes);
                if (interMethod == null) continue;

                byte? icidn = null;
                if (inter.GetNetId(out var icid))
                    icidn = icid;
                tokens.AddRange(interMethod.GetCustomAttributes(false).OfType<TAttr>().Select(x => (icidn, x)));
            }
        }

        #region subscribe tokens
        private static void SubscribeTokens<TAttr>(IRpcProvider provider, IEnumerable<(byte?, TAttr)> tokens, Action<NetMessage> del)
             where TAttr : Attribute, IRpcAttribute
        {
            foreach (var token in tokens)
            {
                if (token.Item2 == null)
                    continue;
                provider.SubscribeToRpc(token.Item2.RpcId, del);
            }
        }

        private static void SubscribeTokens<T, TAttr>(IInfoRpcProvider<T> provider, IEnumerable<(byte?, TAttr)> tokens,
            Action<NetMessage, T> del) where TAttr : Attribute, IRpcAttribute
        {
            foreach (var token in tokens)
            {

                if (token.Item2 == null)
                    continue;
                provider.SubscribeToRpc(token.Item2.RpcId, del, defaultContinueForwarding: token.Item2.DefaultContinueForwarding);
            }
        }

        private static void SubscribeTokens<T, TAttr>(IComponentInfoRpcProvider<T> provider, byte compId, IEnumerable<(byte?, TAttr)> tokens,
            Action<NetMessage, T> del) where TAttr : Attribute, IRpcAttribute
        {
            foreach (var token in tokens)
            {
                if (token.Item2 == null)
                    continue;
                provider.SubscribeToRpc(token.Item1 ?? compId, token.Item2.RpcId, del, defaultContinueForwarding: token.Item2.DefaultContinueForwarding);
            }
        }

        private static void SubscribeTokens<TAttr>(IComponentRpcProvider provider, byte compId, IEnumerable<(byte?, TAttr)> tokens, Action<NetMessage> del)
            where TAttr : Attribute, IRpcAttribute
        {
            foreach (var token in tokens)
            {
                if (token.Item2 == null)
                    continue;
                provider.SubscribeToRpc(token.Item1 ?? compId, token.Item2.RpcId, del);
            }
        }

        private static void SubscribeTokens<TAttr>(IComponentRpcProvider provider, byte compId, IEnumerable<(byte?, TAttr)> tokens, Func<NetMessage, object> fncDel)
            where TAttr : Attribute, IRpcAttribute
        {
            foreach (var token in tokens)
            {
                if (token.Item2 == null)
                    continue;
                provider.SubscribeToFunc(token.Item1 ?? compId, token.Item2.RpcId, fncDel);
            }
        }

        private static void SubscribeTokens<T, TAttr>(IComponentInfoRpcProvider<T> provider, byte compId, IEnumerable<(byte?, TAttr)> tokens, Func<NetMessage, T, object> fncDel)
            where TAttr : Attribute, IRpcAttribute
        {
            foreach (var token in tokens)
            {
                if (token.Item2 == null)
                    continue;
                provider.SubscribeToFunc(token.Item1 ?? compId, token.Item2.RpcId, fncDel);
            }
        }
        #endregion
    }

    /// <summary>
    /// A provider that has no caller info given to the rpc subscription
    /// </summary>
    public interface IRpcProvider
    {
        bool SubscribeToRpc(byte rpcId, Action<NetMessage> rpc);
        void UnsubscribeRpc(byte rpcId);
        void SubscribeRpcsOnObject(object obj);

        void ClearSubscriptions();
    }

    /// <summary>
    /// A provider that has caller information given to the rpc subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInfoRpcProvider<T>
    {
        bool SubscribeToRpc(byte rpcId, Action<NetMessage, T> action, bool overwriteExisting = true, bool defaultContinueForwarding = true);
        void UnsubscribeRpc(byte rpcId);
        void SubscribeRpcsOnObject(object obj);

        void ClearSubscriptions();
    }

    /// <summary>
    /// A provider associated with components that has no caller info given to the subscription
    /// </summary>
    public interface IComponentRpcProvider
    {
        bool SubscribeToRpc(byte componentId, byte rpcID, Action<NetMessage> rpcProcessor, bool overwriteExisting = true,
            bool defaultContinueForwarding = true);

        bool SubscribeToFunc(byte componentId, byte rpcId, Func<NetMessage, object> func, bool overwriteExisting = true);

        void UnsubscribeFromRpc(byte componentId, byte rpcId);
        void UnsubscribeFromRpcs(byte componentId);

        void SubscribeMarkedRpcsOnComponent(object component);

        void ClearSubscriptions();
    }

    /// <summary>
    /// A provider associated with components that has caller info given to the subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IComponentInfoRpcProvider<T>
    {
        bool SubscribeToRpc(byte componentId, byte rpcID, Action<NetMessage, T> rpcProcessor,
            bool overwriteExisting = true, bool defaultContinueForwarding = true);

        bool SubscribeToFunc(byte componentId, byte rpcId, Func<NetMessage, T, object> func,
            bool overwriteExisting = true);

        void UnsubscribeFromRpc(byte componentId, byte rpcID);
        void UnsubscribeFromRpcs(byte componentId);

        void SubscribeMarkedRpcsOnComponent(object component);

        void ClearSubscriptions();
    }

    internal interface IProxyCollection<T>
    {
        void AddProxy(T proxy);
        void RemoveProxy(T proxy);
        void RemoveProxy<TProxy>();
        void ClearProxies();
        TRet Proxy<TRet>();
    }

    internal interface IProxySingle<T>
    {
        void Proxy(T proxy);
        TRet Proxy<TRet>();
    }
}
