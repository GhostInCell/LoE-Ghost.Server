using System.Collections.Generic;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ThreatMgr : ObjectComponent
    {
        protected readonly object _lock = new object();
        protected Dictionary<CreatureObject, float> _threat;
        protected CreatureObject _creature;
        public ThreatMgr(CreatureObject parent) 
            : base(parent)
        {
            _creature = parent;
            _threat = new Dictionary<CreatureObject, float>();
            parent.OnDespawn += ThreatMgr_OnDespawn;
            parent.OnDestroy += ThreatMgr_OnDestroy;
            parent.OnInitialize += ThreatMgr_OnInitialize;
        }
        public void Clear()
        {
            lock (_lock)
            {
                foreach (var item in _threat)
                    _creature.View.SetCombat(item.Key.Owner, false);
                _threat.Clear();
            }
        }
        public void Remove(CreatureObject creature)
        {
            lock (_lock)
                _threat.Remove(creature);
            _creature.View.SetCombat(creature.Owner, false);
        }
        public abstract bool SelectTarget(out CreatureObject target);
        #region Events Handlers
        private void ThreatMgr_OnDespawn()
        {
            Clear();
        }
        private void ThreatMgr_OnDestroy()
        {
            Clear();
            _threat = null;
        }
        private void ThreatMgr_OnInitialize()
        {
            _creature.Stats.OnDamageReceived += ThreatMgr_OnDamageReceived;
        }
        private void ThreatMgr_OnDamageReceived(CreatureObject arg1, float arg2)
        {
            lock (_lock)
            {
                if (_threat.ContainsKey(arg1))
                    _threat[arg1] += arg2;
                else
                {
                    _threat[arg1] = arg2;
                    _creature.View.SetCombat(arg1.Owner, true);
                }
            }
        }
        #endregion
    }
}
