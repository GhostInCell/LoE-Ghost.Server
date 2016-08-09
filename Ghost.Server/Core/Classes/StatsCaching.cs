using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Classes
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

        public float GetMultipler(CreatureMultipler index)
        {
            return m_cache[(int)index];
        }

        public void SetMultipler(CreatureMultipler index, float value)
        {
            m_cache[(int)index] = value;
        }

        public unsafe bool CurrentEqualMax(Stats stat)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
                return ptr[(int)StatIndex.Cur] == ptr[(int)StatIndex.Max];
        }

        public float GetStat(Stats stat, StatIndex index)
        {
            return m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length + (int)index];
        }

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
}