using Ghost.Server.Mgrs.Map;
using PNetR;
using System;
using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class CreatureObject : WorldObject
    {
        protected bool _dead;
        protected StatsMgr _stats;
        protected NetworkView _view;
        protected MovementGenerator _movement;
        public bool IsDead
        {
            get { return _dead; }
        }
        public Player Owner
        {
            get { return _view.Owner; }
        }
        public bool HasStats
        {
            get { return _stats != null; }
        }
        public StatsMgr Stats
        {
            get { return _stats; }
        }
        public NetworkView View
        {
            get { return _view; }
        }
        public override ushort SGuid
        {
            get
            {
                return _view.Id.Id;
            }
        }
        public override Vector3 Position
        {
            get { return _movement.Position; }
            set { _movement.Position = value; }
        }
        public override Vector3 Rotation
        {
            get { return _movement.Rotation; }
            set { _movement.Rotation = value; }
        }
        public MovementGenerator Movement
        {
            get { return _movement; }
        }
        public abstract Vector3 SpawnPosition
        {
            get;
        }
        public abstract Vector3 SpawnRotation
        {
            get;
        }
        public event Action<CreatureObject> OnKilled;
        public CreatureObject(uint guid, ObjectsMgr manager)
            : base(Constants.CreatureObject | guid, manager)
        {
            OnSpawn += CreatureObject_OnSpawn;
            OnDespawn += CreatureObject_OnDespawn;
            OnDestroy += CreatureObject_OnDestroy;
            OnInitialize += CreatureObject_OnInitialize;
        }
        public void Kill(CreatureObject killer)
        {
            lock (this)
            {
                if (_dead) return;
                _dead = true;
            }
            OnKilled?.Invoke(killer);
        }
        #region Events Handlers
        private void CreatureObject_OnSpawn()
        {
            _dead = false;
        }
        private void CreatureObject_OnDespawn()
        {
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view, IsPlayer ? Constants.Fainted : Constants.Killed);
        }
        private void CreatureObject_OnDestroy()
        {
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view, _dead ? Constants.Killed : (byte)0);
            _view = null;
            _stats = null;
            _movement = null;
        }
        private void CreatureObject_OnInitialize()
        {
            _stats = GetComponent<StatsMgr>();
            _movement = GetComponent<MovementGenerator>();
        }
        #endregion
    }
}