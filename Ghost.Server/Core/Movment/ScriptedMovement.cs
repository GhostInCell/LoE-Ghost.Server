using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Numerics;
using System.Threading;

namespace Ghost.Server.Core.Movment
{
    public class ScriptedMovement : MovementGenerator, IUpdatable
    {
        private readonly DB_Movement _entries;
        private int _update;
        private int _locked;
        private ushort _state;
        private bool _waiting;
        private SyncEntry _entry;
        private MovementWait _wait;
        private MovementEntry _current;
        public ushort State
        {
            get { return _state; }
            set
            {
                _state = value;
                if (_waiting)
                {
                    _waiting = false;
                    _wait?.Destroy();
                    _wait = null;
                }
                Execute();
            }
        }
        public override int Animation
        {
            get { return 0; }
        }
        public override bool IsLocked
        {
            get
            {
                return _locked > 0;
            }
        }
        public override bool IsFlying
        {
            get
            {
                return false;
            }
        }
        public override bool IsRunning
        {
            get
            {
                return false;
            }
        }
        public override bool IsMovable
        {
            get
            {
                return true;
            }
        }
        public ScriptedMovement(ushort id, CreatureObject parent)
            : base(parent)
        {
            _state = 0;
            _entry = new SyncEntry();
            _direction = Vector3.UnitZ;
            _position = _creature.SpawnPosition;
            _rotation = _creature.SpawnRotation.ToRadians();
            if (DataMgr.Select(id, out _entries))
                Execute();
            parent.OnSpawn += ScriptedMovement_OnSpawn;
            parent.OnDestroy += ScriptedMovement_OnDestroy;
        }
        public ScriptedMovement(ScriptedMovement original, CreatureObject clone)
            : base(clone)
        {
            _speed = original._speed;
            _state = original._state;
            _entry = new SyncEntry();
            _entries = original._entries;
            _current = original._current;
            _waiting = original._waiting;
            _position = original.Position;
            _rotation = original.Rotation;
            _direction = original._direction;
            clone.OnSpawn += ScriptedMovement_OnSpawn;
            clone.OnDestroy += ScriptedMovement_OnDestroy;
        }
        public void ResetWait()
        {
            _waiting = false;
            _wait = null;
        }
        public override void Unlock()
        {
            if (Interlocked.Decrement(ref _locked) <= 0)
            {
                _direction = Vector3.Normalize(_current.Position - _position);
                _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
            }
        }
        public void Update(TimeSpan time)
        {
            if (_locked <= 0 && _current != null)
            {
                if (_position != _current.Position)
                {
                    var offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                    if (Vector3.DistanceSquared(_position, _current.Position) > (offset.LengthSquared() + Constants.EpsilonX1))
                        _position += offset;
                    else
                    {
                        if (!_waiting)
                        {
                            Execute();
                            offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                            _position += offset;
                        }
                        else
                            _position = _current.Position;
                    }
                }
                else if (_waiting)
                {
                    _rotation = _current.Rotation.ToRadians();
                    if (_wait == null && _current.Data01 > 0)
                        _wait = new MovementWait(this, (_current.Data01 < _current.Data02 ?
                            TimeSpan.FromSeconds(Constants.RND.Next(_current.Data01, _current.Data02)) :
                            TimeSpan.FromSeconds(_current.Data01)));
                }
                else
                    Execute();
            }
            if ((_update -= time.Milliseconds) <= 0)
            {
                _update = _interval;
                _entry.Position = _position;
                _entry.Rotation = _rotation;
                _entry.Time = PNet.Utilities.Now;
                var msg = _creature.View.CreateStream(_entry.AllocSize);
                _entry.OnSerialize(msg); _creature.View.SendStream(msg);
            }
        }
        public override void Lock(bool reset = true)
        {
            Interlocked.Increment(ref _locked);
        }
        public override void LookAt(WorldObject obj)
        {
            if (obj.Position == _position) return;
            _direction = Vector3.Normalize(obj.Position - _position);
            _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
        }
        private void Execute()
        {
            if (_entries.Entries.TryGetValue(_state++, out _current))
            {
                if (_current.Position != _position)
                {
                    _direction = Vector3.Normalize(_current.Position - _position);
                    _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
                }
                switch (_current.Type)
                {
                    case MovementType.Stay:
                        _waiting = true;
                        break;
                }
                switch (_current.Command)
                {
                    case MovementCommand.SetSpeed:
                        _speed = _current.CommandData01;
                        break;
                    case MovementCommand.GoTo:
                        _state = (ushort)_current.CommandData01;
                        break;
                }
            }
            else Interlocked.Increment(ref _locked);
        }
        #region Events Handlers
        private void ScriptedMovement_OnSpawn()
        {
            _creature.View.GettingPosition += View_GettingPosition;
            _creature.View.GettingRotation += View_GettingRotation;
        }
        private void ScriptedMovement_OnDestroy()
        {
            _wait?.Destroy();
            _wait = null;
            _entry = null;
            _current = null;
        }
        private Vector3 View_GettingRotation()
        {
            return _rotation.ToDegrees();
        }
        private Vector3 View_GettingPosition()
        {
            return _position;
        }
        #endregion
    }
}