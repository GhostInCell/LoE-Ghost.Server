using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    public class MobMovement : MovementGenerator, IUpdatable
    {
        private WO_MOB _mob;
        private bool _locked;
        private SyncEntry _entry;
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
                if (_mob.Target != null && _mob.Target.IsSpawned)
                {
                    _direction = Vector3.Normalize(_position - _mob.Target.Position);
                    _rotation = MathHelper.GetRotation(-Vector3.UnitZ, _direction, Vector3.UnitY).QuatToEul2();
                    if (Vector3.Distance(_position, _mob.Target.Position) >= (Constants.MeleeCombatDistance + 0.1f))
                    {
                        _position += (-_direction * (time.Milliseconds * (_mob.Stats.Speed / 50000f)));
                        if (Vector3.Distance(_position, _mob.Target.Position) < Constants.MeleeCombatDistance)
                            _position = _mob.Target.Position + (_direction * Constants.MeleeCombatDistance);
                    }
                }
                else if (Vector3.Distance(_position, _mob.SpawnPosition) > 0)
                {
                    _direction = Vector3.Normalize(_position - _mob.SpawnPosition);
                    _rotation = MathHelper.GetRotation(-Vector3.UnitZ, _direction, Vector3.UnitY).QuatToEul2();
                    if (Vector3.Distance(_position, _mob.SpawnPosition) > 0)
                    {
                        _position += (-_direction * (time.Milliseconds * (_mob.Stats.Speed / 50000f)));
                        if (Vector3.Distance(_position, _mob.SpawnPosition) < Constants.MeleeCombatDistance)
                            _position = _mob.SpawnPosition;
                    }
                }
            }
            _entry.Position = _position;
            _entry.Rotation = _rotation;
            _entry.Time = PNet.Utilities.Now;
            var msg = _mob.View.CreateStream(_entry.AllocSize);
            _entry.OnSerialize(msg); _mob.View.SendStream(msg);
        }
        public override void Lock(bool reset = true)
        {
            _locked = true;
        }
        #region Events Handlers
        private void MobMovement_OnSpawn()
        {
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