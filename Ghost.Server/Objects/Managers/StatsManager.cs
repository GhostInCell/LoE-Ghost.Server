using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using PNet;
using System;

namespace Ghost.Server.Objects.Managers
{
    [NetComponent(4)]
    public class StatsManager : NetworkManager<CreatureObject>
    {
        protected const int SendHealthFlag = 0x00000001;
        protected const int SendEnergyFlag = 0x00000002;
        protected const int SendTensionFlag = 0x00000004;

        private StatsCaching m_caching;

        public bool SendHealth
        {
            get
            {
                return (m_state & SendHealthFlag) != 0;
            }
            set
            {
                m_state = value ? m_state | SendHealthFlag : m_state & ~SendHealthFlag;
            }
        }

        public bool SendEnergy
        {
            get
            {
                return (m_state & SendEnergyFlag) != 0;
            }
            set
            {
                m_state = value ? m_state | SendEnergyFlag : m_state & ~SendEnergyFlag;
            }
        }

        public bool SendTension
        {
            get
            {
                return (m_state & SendTensionFlag) != 0;
            }
            set
            {
                m_state = value ? m_state | SendTensionFlag : m_state & ~SendTensionFlag;
            }
        }

        public StatsCaching Caching
        {
            get { return m_caching; }
        }

        public StatsManager()
            : base()
        {
            m_caching = new StatsCaching();
            m_state |= SendHealthFlag | SendEnergyFlag;
        }
        #region Overridden Methods
        protected override void OnViewCreated()
        {
            base.OnViewCreated();
            m_view.SubscribeMarkedRpcsOnComponent(this);
        }

        protected override void OnUpdate(TimeSpan time)
        {
            base.OnUpdate(time);
            var delta = (time.Milliseconds * 0.001f);
            var caching = m_caching;
            caching.UpdateCurrent(Stats.Health, Stats.HealthRegen, delta);
            caching.UpdateCurrent(Stats.Energy, Stats.EnergyRegen, delta);
            var state = m_state & (SendHealthFlag | SendEnergyFlag | SendTensionFlag);
            if (state != 0)
            {
                var view = m_view;
                var sender = default(StatNetData);
                if ((state & SendHealthFlag) != 0)
                    view.Rpc(4, 50, RpcMode.AllUnordered, sender.Fill(Stats.Health, caching.GetStat(Stats.Health, StatIndex.Cur)));
                if ((state & SendEnergyFlag) != 0)
                    view.Rpc(4, 50, RpcMode.AllUnordered, sender.Fill(Stats.Energy, caching.GetStat(Stats.Energy, StatIndex.Cur)));
                if ((state & SendTensionFlag) != 0)
                    view.Rpc(4, 50, RpcMode.AllUnordered, sender.Fill(Stats.Tension, caching.GetStat(Stats.Tension, StatIndex.Cur)));
            }
        }
        #endregion
    }
}