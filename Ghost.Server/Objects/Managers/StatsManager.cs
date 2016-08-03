using Ghost.Server.Utilities;
using PNet;
using System;

namespace Ghost.Server.Objects.Managers
{

    public class StatsCaching
    {
        private struct StatValue
        {
            public float Mul;
            public float Mod;
            public float Min;
            public float Cur;
            public float Max;
            public float Base;
            public float Item;
        }

        private float[] m_cache;

        public float[] Chache
        {
            get { return m_cache; }
        }

        public float this[int index]
        {
            get { return m_cache[index]; }
            set { m_cache[index] = value; }
        }

        public StatsCaching()
        {
            m_cache = new float[(int)StatsOffset.Length];
        }

        public unsafe bool CurrentEqualMax(Stats stat)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
                return ptr[(int)StatIndex.Cur] == ptr[(int)StatIndex.Max];
        }

        //public float GetValue<TEnum>(TEnum index)
        //    where TEnum : struct, IConvertible
        //{
        //    return m_cache[index.ToInt32(CultureInfo.InvariantCulture)];
        //}

        public float GetStat(Stats stat, StatIndex index)
        {
            return m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length + (int)index];
        }

        //public void SetValue<TEnum>(TEnum index, float value)
        //    where TEnum : struct, IConvertible
        //{
        //    m_cache[index.ToInt32(CultureInfo.InvariantCulture)] = value;
        //}

        public void SetStat(Stats stat, StatIndex index, float value)
        {
            m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length + (int)index] = value;
        }

        public unsafe void Recalculate(Stats stat)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                var coff = statPtr->Cur / statPtr->Max;
                statPtr->Max = (statPtr->Base + statPtr->Item + statPtr->Mod) * statPtr->Mul;
                if (statPtr->Max < statPtr->Min) statPtr->Max = statPtr->Min;
                if (statPtr->Cur > 0) statPtr->Cur = statPtr->Max * coff;
            }
        }

        public unsafe void Initialize(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Mul = 1f;
                statPtr->Cur = value;
                statPtr->Max = value;
                statPtr->Base = value;
            }
        }

        public unsafe void IncreaseCurrent(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Cur = MathHelper.Clamp(statPtr->Cur + value, statPtr->Min, statPtr->Max);
            }
        }

        public unsafe void DecreaseCurrent(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Cur = MathHelper.Clamp(statPtr->Cur - value, statPtr->Min, statPtr->Max);
            }
        }
    }

    [NetComponent(4)]
    public class StatsManager : NetworkManager<CreatureObject>
    {
        private struct StatSender : INetSerializable
        {
            public Stats Stat;
            public float Value;

            public int AllocSize
            {
                get
                {
                    return 5;
                }
            }

            public void OnSerialize(NetMessage message)
            {
                message.Write((byte)Stat);
                message.Write(Value);
            }

            public void OnDeserialize(NetMessage message)
            {
                Stat = (Stats)message.ReadByte();
                Value = message.ReadSingle();
            }

            public StatSender Fill(Stats stat, float value)
            {
                Stat = stat;
                Value = value;
                return this;
            }
        }

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
            if (!caching.CurrentEqualMax(Stats.Health))
                caching.IncreaseCurrent(Stats.Health, caching.GetStat(Stats.HealthRegen, StatIndex.Max) * delta);
            if (!caching.CurrentEqualMax(Stats.Energy))
                caching.IncreaseCurrent(Stats.Energy, caching.GetStat(Stats.EnergyRegen, StatIndex.Max) * delta);
            var state = m_state & (SendHealthFlag | SendEnergyFlag | SendTensionFlag);
            if (state != 0)
            {
                var view = m_view;
                var sender = default(StatSender);
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