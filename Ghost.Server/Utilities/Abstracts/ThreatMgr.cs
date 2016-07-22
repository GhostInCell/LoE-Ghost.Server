using System.Collections.Generic;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ThreatMgr : ObjectComponent
    {
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
            foreach (var item in _threat)
                _creature.View.SetCombat(item.Key.Owner, false);
            _threat.Clear();
        }
        public void Remove(CreatureObject creature)
        {
            _threat.Remove(creature);
            creature.View.SetCombat(creature.Owner, false);
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
            if (_threat.ContainsKey(arg1))
                _threat[arg1] += arg2;
            else
            {
                _threat[arg1] = arg2;
                arg1.View.SetCombat(arg1.Owner, true);
            }
        }
        #endregion
    }
}