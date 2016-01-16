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
        private bool _enabled;
        private bool _updating;
        private bool _spawned;
        private bool _initialized;
        private int _updatableLength;
        private int _componentsLength;
        private IUpdatable[] _updatable;
        private ObjectComponent[] _components;
        public uint Guid
        {
            get
            {
                return _guid;
            }
        }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                for (int i = 0; i < _components.Length; i++)
                    _components[i].Enabled = value;
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
        public abstract byte TypeID { get; }
        public abstract ushort SGuid { get; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }
        public WorldObject(uint guid, ObjectsMgr manager)
        {
            _enabled = true;
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
            if (_updatable.Length > 0)
                _server.RigisterOnUpdate(this);
        }
        public void Despawn()
        {
            _spawned = false;
            _manager.RemoveView(this);
            if (_updatable.Length > 0)
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
            if (_updatable.Length > 0)
                _server.RemoveFromUpdate(this);
            OnDestroy?.Invoke();
            _server = null;
            _manager = null;
            _updatable = null;
            _components = null;
        }
        //public void Enable<T>()
        //    where T : ObjectComponent, IUpdatable
        //{
        //    for (int i = 0; i < _componentsLength; i++)
        //        if (_components[i] is T)
        //            RegisterToUpdate((T)_components[i]);
        //}
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
        //public void RemoveComponent<T>()
        //{
        //    throw new NotImplementedException();
        //}
        public void Update(TimeSpan time)
        {
            if (!_enabled || !_spawned) return;
            for (int i = 0; i < _updatable.Length; i++)
                if (_updatable[i].Enabled)
                    _updatable[i].Update(time);
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
        private void RegisterToUpdate<T>(T component)
            where T : IUpdatable
        {

        }
        private void RemoveFromUpdate<T>(T component)
            where T : IUpdatable
        {

        }
    }
}