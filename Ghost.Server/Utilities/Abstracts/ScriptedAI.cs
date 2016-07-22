using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ScriptedAI : ObjectComponent, IUpdatable
    {
        protected StatsMgr _stats;
        protected ThreatMgr _threat;
        protected CreatureObject _target;
        protected CreatureObject _creature;
        protected MovementGenerator _movement;
        private float _aTime;
        private TargetEntry _tEntry;
        public CreatureObject Target
        {
            get { return _target; }
        }
        public ScriptedAI(CreatureObject parent)
            : base(parent)
        {
            _creature = parent;
            parent.OnDestroy += ScriptedAI_OnDestroy;
            parent.OnInitialize += ScriptedAI_OnInitialize;
        }
        public void DoMeleeAttackIfReady()
        {
            if (_target != null && _aTime <= 0)
            {
                _aTime = _stats.AttackRate;
                if (_target.IsDead)
                {
                    _threat.Remove(_target);
                    _target = null;
                }
                else if (Vector3.DistanceSquared(_movement.Position, _target.Position) <= (Constants.MaxMeleeCombatDistanceSquared + Constants.EpsilonX1))
                {
                    _creature.View.PerformSkill(_tEntry.Fill(_target));
                    _target.Stats.DoDamage(_creature, _stats.MeleeDamage);
                    if (_target.IsDead) _threat.Remove(_target);
                }
            }
        }
        public void Update(TimeSpan time)
        {
            if (_aTime > 0)
                _aTime -= time.Milliseconds;
            OnUpdate(time);
        }
        public abstract void OnUpdate(TimeSpan time);
        #region Events Handlers
        private void ScriptedAI_OnDestroy()
        {
            _stats = null;
            _threat = null;
            _tEntry = null;
            _target = null;
            _creature = null;
            _movement = null;
        }
        private void ScriptedAI_OnInitialize()
        {
            _stats = _parent.RequiredComponent<StatsMgr>();
            _threat = _parent.RequiredComponent<ThreatMgr>();
            _movement = _parent.RequiredComponent<MovementGenerator>();
            _tEntry = new TargetEntry() { SpellID = _stats.MeleeSkill, Upgrade = 0 };
        }
        #endregion
    }
}