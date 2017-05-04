using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ThreatMgr : ObjectComponent
    {
        protected CreatureObject m_creature;

        protected ConcurrentDictionary<CreatureObject, float> m_threat;

        public ThreatMgr(CreatureObject parent) 
            : base(parent)
        {
            m_creature = parent;
            m_threat = new ConcurrentDictionary<CreatureObject, float>();
            parent.OnDespawn += ThreatMgr_OnDespawn;
            parent.OnDestroy += ThreatMgr_OnDestroy;
            parent.OnInitialize += ThreatMgr_OnInitialize;
        }
        public void Clear()
        {
            foreach (var item in m_threat)
                item.Key.View.SetCombat(item.Key.Owner, false);
            m_threat.Clear();
        }
        public void Remove(CreatureObject creature)
        {
            m_threat.TryRemove(creature, out _);
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
            m_threat = null;
        }
        private void ThreatMgr_OnInitialize()
        {
            m_creature.Stats.OnDamageReceived += ThreatMgr_OnDamageReceived;
        }
        private void ThreatMgr_OnDamageReceived(CreatureObject arg1, float arg2)
        {
            if (m_threat.ContainsKey(arg1))
                m_threat[arg1] += arg2;
            else
            {
                m_threat[arg1] = arg2;
                arg1.View.SetCombat(arg1.Owner, true);
            }
        }
        #endregion
    }
}