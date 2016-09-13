using Ghost.Server.Utilities;
using System.Runtime.CompilerServices;

namespace Ghost.Server.Core.Classes
{
    public unsafe class StatsCaching
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

        public void UpdateCurrent(Stats stat01, Stats stat02, float delta)
        {
            fixed (float* ptr = m_cache)
            {
                StatValue* var01 = (StatValue*)(ptr + ((int)StatsOffset.Stats + ((int)stat01 - 1) * (int)StatIndex.Length));
                StatValue* var02 = (StatValue*)(ptr + ((int)StatsOffset.Stats + ((int)stat02 - 1) * (int)StatIndex.Length));
                if (var01->Cur != var02->Cur)
                    var01->Cur = MathHelper.Clamp(var02->Max * delta, var01->Min, var01->Max);
            }
        }

        public void Modifier(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Mod += value;
                Recalculate(statPtr);
            }
        }

        public void Multipler(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Mul += value;
                Recalculate(statPtr);
            }
        }

        public void ItemModifier(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Item += value;
                Recalculate(statPtr);
            }
        }

        public float GetStat(Stats stat, StatIndex index)
        {
            return m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length + (int)index];
        }

        public void Initialize(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Min = 0f;
                statPtr->Mul = 1f;
                statPtr->Cur = value;
                statPtr->Max = value;
                statPtr->Base = value;
            }
        }

        public void IncreaseCurrent(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Cur = MathHelper.Clamp(statPtr->Cur + value, statPtr->Min, statPtr->Max);
            }
        }

        public void DecreaseCurrent(Stats stat, float value)
        {
            fixed (float* ptr = &m_cache[(int)StatsOffset.Stats + ((int)stat - 1) * (int)StatIndex.Length])
            {
                var statPtr = (StatValue*)ptr;
                statPtr->Cur = MathHelper.Clamp(statPtr->Cur - value, statPtr->Min, statPtr->Max);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Recalculate(StatValue* stat)
        {
            var coff = stat->Cur / stat->Max;
            stat->Max = (stat->Base + stat->Item + stat->Mod) * stat->Mul;
            if (stat->Max < stat->Min) stat->Max = stat->Min;
            if (stat->Cur > 0) stat->Cur = stat->Max * coff;
        }
    }
}