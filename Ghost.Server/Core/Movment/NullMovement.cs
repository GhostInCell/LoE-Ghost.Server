using Ghost.Server.Utilities.Abstracts;
using System.Numerics;

namespace Ghost.Server.Core.Movment
{
    public class NullMovement : MovementGenerator
    {
        public NullMovement(CreatureObject parent) 
            : base(parent)
        {
            _position = _creature.SpawnPosition;
            _rotation = _creature.SpawnRotation;
            parent.OnSpawn += NullMovement_OnSpawn;
        }
        public override int Animation
        {
            get { return 0; }
        }
        public override bool IsFlying
        {
            get
            {
                return false;
            }
        }
        public override bool IsLocked
        {
            get
            {
                return true;
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
                return false;
            }
        }
        public override void Unlock()
        {
        }
        public override void Lock(bool reset = true)
        {
        }
        public override void LookAt(WorldObject obj)
        {
        }
        #region Events Handlers
        private void NullMovement_OnSpawn()
        {
            _creature.View.GettingPosition += View_GettingPosition;
            _creature.View.GettingRotation += View_GettingRotation;
        }
        private Vector3 View_GettingRotation()
        {
            return _rotation;
        }
        private Vector3 View_GettingPosition()
        {
            return _position;
        }
        #endregion
    }
}