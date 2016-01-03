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
        public ScriptedMovement(ushort id, CreatureObject obj)
            : base(obj)
        {
            _state = 0;
            _entry = new SyncEntry();
            _direction = Vector3.UnitZ;
            _position = obj.SpawnPosition;
            _rotation = obj.SpawnRotation;
            obj.OnSpawn += ScriptedMovement_OnSpawn;
            obj.OnDespawn += ScriptedMovement_OnDespawn;
            if (DataMgr.Select(id, out _entries))
                Execute();
        }
        public ScriptedMovement(ScriptedMovement original, CreatureObject obj)
            : base(obj)
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
            obj.OnSpawn += ScriptedMovement_OnSpawn;
            obj.OnDespawn += ScriptedMovement_OnDespawn;
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
        public override void Destroy()
        {
            Interlocked.Increment(ref _locked);
            _wait?.Destroy();
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
            _wait = null;
            _entry = null;
            _object = null;
            _current = null;
        }
        public void Update(TimeSpan time)
        {
            if (_locked <= 0 && _current != null)
            {
                if (_position != _current.Position)
                {
                    var offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                    if (Vector3.Distance(_position, _current.Position) > offset.Length())
                        _position += offset;
                    else
                    {
                        _position = _current.Position;
                        if (!_waiting) Execute();
                    }
                }
                else if (_waiting)
                    _rotation = _current.Rotation.ToRadians();
                else
                    Execute();
                _entry.Position = _position;
                _entry.Rotation = _rotation;
                _entry.Time = PNet.Utilities.Now;
                var msg = _object.View.CreateStream(_entry.AllocSize);
                _entry.OnSerialize(msg); _object.View.SendStream(msg);
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
                        if (_current.Data01 > 0)
                            _wait = new MovementWait(this, (_current.Data01 > _current.Data02 ?
                                TimeSpan.FromSeconds(Constants.RND.Next(_current.Data01, _current.Data02)) :
                                TimeSpan.FromSeconds(_current.Data01)));
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
            _object.View.GettingPosition += View_GettingPosition;
            _object.View.GettingRotation += View_GettingRotation;
        }
        private void ScriptedMovement_OnDespawn()
        {
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
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