using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class MovementGenerator : ObjectComponent
    {
        protected static readonly int _interval;
        static MovementGenerator()
        {
            _interval = Configs.Get<int>(Configs.Sync_Movement);
        }
        protected float _speed;
        protected Vector3 _position;
        protected Vector3 _rotation;
        protected Vector3 _direction;
        protected CreatureObject _creature;
        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public Vector3 Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
        public Vector3 Direction
        {
            get { return _direction; }
            set { _direction = Vector3.Normalize(value); }
        }
        public abstract int Animation
        {
            get;
        }
        public abstract bool IsLocked
        {
            get;
        }
        public abstract bool IsFlying
        {
            get;
        }
        public abstract bool IsRunning
        {
            get;
        }
        public abstract bool IsMovable
        {
            get;
        }
        public MovementGenerator(CreatureObject parent)
            : base(parent)
        {
            _creature = parent;
            parent.OnDestroy += MovementGenerator_OnDestroy;
        }
        public abstract void Unlock();
        public abstract void Lock(bool reset = true);
        public abstract void LookAt(WorldObject obj);
        #region Events Handlers
        private void MovementGenerator_OnDestroy()
        {
            _creature = null;
        }
        #endregion
    }
}