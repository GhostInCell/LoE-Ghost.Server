using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities.Interfaces;
using PNetR;
using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class CreatureObject : WorldObject
    {
        protected bool _isKilled;
        protected StatsMgr _stats;
        protected NetworkView _view;
        protected MovementGenerator _movement;
        public StatsMgr Stats
        {
            get { return _stats; }
        }
        public NetworkView View
        {
            get { return _view; }
        }
        public bool HasStats
        {
            get { return _stats != null; }
        }
        public override ushort SGuid
        {
            get
            {
                return _view.Id;
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
        public abstract Vector3 SpawnPosition
        {
            get;
        }
        public abstract Vector3 SpawnRotation
        {
            get;
        }
        public MovementGenerator Movement
        {
            get { return _movement; }
        }
        public CreatureObject(uint guid, ObjectsMgr manager)
            : base(Constants.CreatureObject | guid, manager)
        { }
        #region Events Handlers
        private void CreatureObject_OnSpawn()
        {
            if (_stats != null)
                _server.RigisterOnUpdate(_stats);
            if (_movement is IUpdatable)
                _server.RigisterOnUpdate((IUpdatable)_movement);
        }       
        private void CreatureObject_OnDespawn()
        {
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view, Constants.Killed);
            if (_stats != null)
                _server.RemoveFromUpdate(_stats);
            if (_movement is IUpdatable)
                _server.RemoveFromUpdate((IUpdatable)_movement);
        }
        private void CreatureObject_OnDestroy()
        {
            _view.ClearSubscriptions();
            _server.Room.Destroy(_view, _isKilled ? Constants.Killed : (byte)0);
            if (_stats != null)
            {
                _stats.Destroy();
                _server.RemoveFromUpdate(_stats);
            }
            if (_movement is IUpdatable)
                _server.RemoveFromUpdate((IUpdatable)_movement);
            _movement.Destroy();
        }
        #endregion
    }
}