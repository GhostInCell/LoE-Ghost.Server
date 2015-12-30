using Ghost.Server.Core.Servers;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class WorldObject
    {
        protected readonly uint _guid;
        private bool _spawned;
        protected MapServer _server;
        protected ObjectsMgr _manager;
        internal event Action OnSpawn;
        internal event Action OnDespawn;
        internal event Action OnDestroy;
        public uint Guid
        {
            get
            {
                return _guid;
            }
        }
        public bool IsPlayer
        {
            get
            {
                return (_guid & Constants.PlayerObject) > 0;
            }
        }
        public bool IsServer
        {
            get
            {
                return (_guid & Constants.ServerObject) > 0;
            }
        }
        public bool IsSpawned
        {
            get
            {
                return _spawned;
            }
        }
        public bool IsDynamic
        {
            get
            {
                return (_guid & Constants.DynamicObject) > 0;
            }
        }
        public bool IsCreature
        {
            get
            {
                return (_guid & Constants.CreatureObject) > 0;
            }
        }
        public MapServer Server
        {
            get { return _server; }
        }
        public ObjectsMgr Manager
        {
            get
            {
                return _manager;
            }
        }
        public abstract byte TypeID { get; }
        public abstract ushort SGuid { get; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }
        public WorldObject(uint guid, ObjectsMgr manager)
        {
            _manager = manager;
            _server = manager.Server;
            _guid = guid | (uint)(TypeID << 16);
            _manager.Add(this);
            RegisterEvents();
        }
        public void Spawn()
        {
            _spawned = true;
            OnSpawn?.Invoke();
            _manager.AddView(this);
            if (this is IUpdatable)
                _server.RigisterOnUpdate((IUpdatable)this);
        }
        public void Despawn()
        {
            _spawned = false;
            _manager.RemoveView(this);
            if (this is IUpdatable)
                _server.RemoveFromUpdate((IUpdatable)this);
            OnDespawn?.Invoke();
        }
        public void Destroy()
        {
            _spawned = false;
            _manager.RemoveView(this);
            _manager.Remove(this);
            if ((_guid & Constants.ReleaseGuide) > 0)
                _manager.ReleaseGuid(_guid);
            if (this is IUpdatable)
                _server.RemoveFromUpdate((IUpdatable)this);
            OnDestroy?.Invoke();
            if (OnSpawn != null)
                foreach (var item in OnSpawn.GetInvocationList())
                    OnSpawn -= (Action)item;
            if (OnDestroy != null)
                foreach (var item in OnDestroy.GetInvocationList())
                    OnDestroy -= (Action)item;
            if (OnDespawn != null)
                foreach (var item in OnDespawn.GetInvocationList())
                    OnDespawn -= (Action)item;
            _server = null;
            _manager = null;
        }
        private void RegisterEvents()
        {
            var type = GetType();
            var stack = new Stack<Type>();
            do
            {
                stack.Push(type);
                type = type.BaseType;
            } while (type != typeof(WorldObject));
            while (stack.Count > 0)
            {
                type = stack.Pop();
                foreach (var item in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(x => x.ReturnType == typeof(void) && x.GetParameters().Length == 0 && x.Name.StartsWith(type.Name)))
                {
                    switch (item.Name.Substring(type.Name.Length + 1))
                    {
                        case "OnSpawn":
                            OnSpawn += (Action)item.CreateDelegate(typeof(Action), this);
                            break;
                        case "OnDespawn":
                            OnDespawn += (Action)item.CreateDelegate(typeof(Action), this);
                            break;
                        case "OnDestroy":
                            OnDestroy += (Action)item.CreateDelegate(typeof(Action), this);
                            break;
                    }
                }
            }
        }
    }
}