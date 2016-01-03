using Ghost.Server.Utilities.Interfaces;
using System;
using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class MovementGenerator
    {
        protected float _speed;
        protected Vector3 _position;
        protected Vector3 _rotation;
        protected Vector3 _direction;
        protected CreatureObject _object;
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
            set { _direction = Vector3.Normalize(_direction); }
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
        public abstract bool IsMovable { get; }
        public MovementGenerator(CreatureObject obj)
        {
            _object = obj;
        }
        public abstract void Unlock();
        public abstract void Destroy();
        public abstract void Lock(bool reset = true);
        public abstract void LookAt(WorldObject obj);
    }
}