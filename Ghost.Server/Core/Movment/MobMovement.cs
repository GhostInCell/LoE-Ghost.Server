using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    public class MobMovement : MovementGenerator, IUpdatable
    {
        private int _update;
        private WO_MOB _mob;
        private bool _locked;
        private SyncEntry _entry;
        public override int Animation
        {
            get { return 0; }
        }
        public override bool IsLocked
        {
            get
            {
                return _locked;
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
        public MobMovement(WO_MOB obj) 
            : base(obj)
        {
            _mob = obj;
            _entry = new SyncEntry();
            _position = obj.SpawnPosition;
            _object.OnSpawn += MobMovement_OnSpawn;
            _rotation = obj.SpawnRotation.ToRadians();
            _object.OnDespawn += MobMovement_OnDespawn;
        }
        public override void Destroy()
        {
            _locked = true;
            _object.View.GettingPosition -= View_GettingPosition;
            _object.View.GettingRotation -= View_GettingRotation;
            _mob = null;
            _entry = null;
            _object = null;
        }
        public override void Unlock()
        {
            _locked = false;
        }
        public void Update(TimeSpan time)
        {
            if (!_locked)
            {
                if (_mob.Target?.IsSpawned ?? false)
                {
                    if (Vector3.Distance(_position, _mob.Target.Position) > Constants.MeleeCombatDistance)
                    {
                        _direction = Vector3.Normalize(_mob.Target.Position - _position);
                        _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
                        var offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                        if (Vector3.Distance(_position, _mob.Target.Position) > Constants.MeleeCombatDistance + offset.Length())
                            _position += offset;
                        else
                            _position = _mob.Target.Position - (_direction * (Constants.MeleeCombatDistance - 0.05f));
                    }
                }
                else if (Vector3.Distance(_position, _mob.SpawnPosition) > 0)
                {
                    _direction = Vector3.Normalize(_mob.SpawnPosition - _position);
                    _rotation = MathHelper.GetRotation(Vector3.UnitZ, _direction, Vector3.UnitY);
                    var offset = (_direction * ((time.Milliseconds / 1000f) * _speed / 45f));
                    if (Vector3.Distance(_position, _mob.SpawnPosition) > offset.Length())
                        _position += offset;
                    else
                        _position = _mob.SpawnPosition;
                }
            }
            if ((_update -= time.Milliseconds) <= 0)
            {
                _update = _interval;
                _entry.Position = _position;
                _entry.Rotation = _rotation;
                _entry.Time = PNet.Utilities.Now;
                var msg = _object.View.CreateStream(_entry.AllocSize);
                _entry.OnSerialize(msg); _object.View.SendStream(msg);
            }
        }
        public override void Lock(bool reset = true)
        {
            _locked = true;
        }
        public override void LookAt(WorldObject obj)
        {
        }
        #region Events Handlers
        private void MobMovement_OnSpawn()
        {
            _speed = _object.Stats.Speed;
            _object.View.GettingPosition += View_GettingPosition;
            _object.View.GettingRotation += View_GettingRotation;
        }
        private void MobMovement_OnDespawn()
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