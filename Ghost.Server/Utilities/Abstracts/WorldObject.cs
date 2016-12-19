using Ghost.Server.Core.Servers;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class WorldObject : IUpdatable
    {
        protected readonly object _lock;
        protected readonly uint _guid;
        protected MapServer _server;
        protected ObjectsMgr _manager;

        private bool _spawned;
        private bool _initialized;
        private int _componentsLength;
        private IUpdatable[] _updatable;
        private ObjectComponent[] _components;
        private Action<TimeSpan> eventOnUpdate;
        public uint Guid
        {
            get
            {
                return _guid;
            }
        }
        public bool IsClone
        {
            get
            {
                return (_guid & Constants.ClonedObject) > 0;
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
        public event Action OnSpawn;
        public event Action OnDespawn;
        public event Action OnDestroy;
        public event Action OnInitialize;
        public event Action<TimeSpan> OnUpdate
        {
            add
            {
                if (_updatable?.Length == 0 && eventOnUpdate == null)
                    _server.RigisterOnUpdate(this);
                eventOnUpdate += value;
            }
            remove
            {
                eventOnUpdate -= value;
                if (_updatable.Length == 0 && eventOnUpdate == null)
                    _server.RemoveFromUpdate(this);
            }
        }
        public abstract byte TypeID { get; }
        public abstract ushort SGuid { get; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }

        public WorldObject(uint guid, ObjectsMgr manager)
        {
            _manager = manager;
            _lock = new object();
            _server = manager.Server;
            _guid = guid | (uint)(TypeID << 16);
            _components = new ObjectComponent[Constants.ArrayCapacity];
            _manager.Add(this);
        }
        public void Spawn()
        {
            _spawned = true;
            if (!_initialized)
                Initialize();
            OnSpawn?.Invoke();
            _manager.AddView(this);
            if (_updatable.Length > 0 || eventOnUpdate != null)
                _server.RigisterOnUpdate(this);
        }
        public void Despawn()
        {
            _spawned = false;
            _manager.RemoveView(this);
            if (_updatable.Length > 0 || eventOnUpdate != null)
                _server.RemoveFromUpdate(this);
            OnDespawn?.Invoke();
        }
        public void Destroy()
        {
            _spawned = false;
            _manager.RemoveView(this);
            _manager.Remove(this);
            if ((_guid & Constants.ReleaseGuide) > 0)
                _manager.ReleaseGuid(_guid);
            if (_updatable.Length > 0 || eventOnUpdate != null)
                _server.RemoveFromUpdate(this);
            OnDestroy?.Invoke();
            CleanEvents();
            _server = null;
            _manager = null;
            _updatable = null;
            _components = null;
        }
        public T GetComponent<T>()
            where T : class
        {
            for (int i = 0; i < _components.Length; i++)
                if (_components[i] is T) return _components[i] as T;
            return null;
        }
        public T RequiredComponent<T>()
            where T : class
        {
            for (int i = 0; i < _components.Length; i++)
                if (_components[i] is T) return _components[i] as T;
            throw new InvalidOperationException();
        }
        public void Update(TimeSpan time)
        {
            if (!_spawned) return;
            for (int i = 0; i < _updatable.Length; i++)
                _updatable[i].Update(time);
            eventOnUpdate?.Invoke(time);
        }
        public void AddComponent<T>(T component)
            where T : ObjectComponent
        {
            if (_initialized)
                throw new NotImplementedException();
            _components[_componentsLength++] = component;
            if (_componentsLength == _components.Length)
                Array.Resize(ref _components, _components.Length + Constants.ArrayCapacity);
        }
        private void Initialize()
        {
            int index = 0;
            Array.Resize(ref _components, _componentsLength);
            _updatable = new IUpdatable[_components.Length];
            foreach (var component in _components)
                if (component is IUpdatable)
                    _updatable[index++] = (IUpdatable)component;
            if (_updatable.Length != index)
                Array.Resize(ref _updatable, index);
            OnInitialize?.Invoke();
            _initialized = true;
        }
        private void CleanEvents()
        {
            {
                var handler = OnSpawn;
                if (handler != null)
                {
                    foreach (var item in handler.GetInvocationList())
                        handler -= (Action)item;
                }
            }
            {
                var handler = OnDespawn;
                if (handler != null)
                {
                    foreach (var item in handler.GetInvocationList())
                        handler -= (Action)item;
                }
            }
            {
                var handler = OnDestroy;
                if (handler != null)
                {
                    foreach (var item in handler.GetInvocationList())
                        handler -= (Action)item;
                }
            }
            {
                var handler = OnInitialize;
                if (handler != null)
                {
                    foreach (var item in handler.GetInvocationList())
                        handler -= (Action)item;
                }
            }
            {
                var handler = eventOnUpdate;
                if (handler != null)
                {
                    foreach (var item in handler.GetInvocationList())
                        handler -= (Action<TimeSpan>)item;
                }
            }
        }
    }
}