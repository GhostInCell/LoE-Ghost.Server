using Ghost.Server.Core.Classes;
using Ghost.Server.Spells;
using Ghost.Server.Utilities;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Objects.Managers
{
    public class SpellCastArgs : BaseArgs
    {
        public const uint FailEvent = 0x88000001;

        public SpellCastResult Result;
    }

    [NetComponent(5)]
    public class SpellsManager : NetworkManager<CreatureObject>
    {
        private class CountdownList
        {
            private const int GrowingFactor = 4;

            private int m_size;
            private int[] m_keys;
            private int[] m_values;

            public CountdownList()
            {
                m_size = 0;
                m_keys = ArrayEx.Empty<int>();
                m_values = ArrayEx.Empty<int>();
            }

            public void Clear()
            {
                Array.Clear(m_keys, 0, m_size);
                Array.Clear(m_values, 0, m_size);
                m_size = 0;
            }

            public void TrimExcess()
            {
                int threshold = (int)(m_keys.Length * 0.9f);
                if (m_size < threshold)
                    ResizeBuffers(m_size);
            }

            public void Remove(int key)
            {
                var index = Array.BinarySearch(m_keys, 0, m_size, key);
                if (index >= 0)
                    RemoveAt(index);
            }

            public int GetValue(int key)
            {
                var index = Array.BinarySearch(m_keys, 0, m_size, key);
                if (index >= 0)
                    return m_values[index];
                return -1;
            }

            public bool Contains(int key)
            {
                return Array.BinarySearch(m_keys, 0, m_size, key) >= 0;
            }

            public void Update(int milliseconds)
            {
                for (int index = m_size - 1; index >= 0; index--)
                {
                    if ((m_values[index] -= milliseconds) <= 0)
                        RemoveAt(index);
                }
            }

            private void RemoveAt(int index)
            {
                m_size--;
                Array.Copy(m_keys, index + 1, m_keys, index, m_size - index);
                Array.Copy(m_values, index + 1, m_values, index, m_size - index);
                m_keys[m_size] = 0;
                m_values[m_size] = 0;
            }

            private void ResizeBuffers(int newLength)
            {
                if (newLength < 0)
                    throw new ArgumentOutOfRangeException();
                else if (newLength == 0)
                {
                    m_keys = ArrayEx.Empty<int>();
                    m_values = ArrayEx.Empty<int>();
                }
                else
                {
                    Array.Resize(ref m_keys, newLength);
                    Array.Resize(ref m_values, newLength);
                }
            }

            public void AddOrUpdate(int key, int time)
            {
                var index = Array.BinarySearch(m_keys, 0, m_size, key);
                if (index >= 0)
                {
                    var value = m_values[index] + time;
                    if (value <= 0)
                        RemoveAt(index);
                    else
                        m_values[index] = value;
                }
                else
                    Insert(~index, key, time);
            }

            private void Insert(int index, int key, int time)
            {
                if (m_size == m_keys.Length)
                    ResizeBuffers(m_keys.Length + GrowingFactor);
                if (index < m_size)
                {
                    Array.Copy(m_keys, index, m_keys, index + 1, m_size - index);
                    Array.Copy(m_values, index, m_values, index + 1, m_size - index);
                }
                m_keys[index] = key;
                m_size++;
            }
        }

        private int m_gcd;
        private SpellCast m_cast;
        private TargetEntry m_target;
        private SpellCastArgs m_cast_args;
        private CountdownList m_cooldowns;
        private Dictionary<int, int> m_spells;

        public SpellCast Cast
        {
            get { return m_cast; }
        }

        public SpellsManager()
            : base()
        {
            m_gcd = 0;
            m_cast = new SpellCast();
            m_target = new TargetEntry();
            m_cooldowns = new CountdownList();
            m_cast_args = new SpellCastArgs();
        }

        public bool CanCast(int id, int upgrade)
        {
            int mUpgrade;
            return m_gcd <= 0 && m_spells.TryGetValue(id, out mUpgrade) && mUpgrade == upgrade && !m_cooldowns.Contains(id) && m_cast.CanCast(id, upgrade);
        }

        #region RPC Handlers
        [Rpc(61, false)]//PerformSkill
        private void RPC_061(NetMessage message, NetMessageInfo info)
        {
            m_target.OnDeserialize(message);
            var castResult = SpellCastResult.Fail;
            if (CanCast(m_target.SpellID, m_target.Upgrade))
                castResult = m_cast.Initialize(m_target.SpellID, m_target.Upgrade, m_target);
            if (castResult != SpellCastResult.OK)
            {
                m_cast_args.Result = castResult;
                m_owner.Notify(SpellCastArgs.FailEvent, m_cast_args);
            }
        }
        [Rpc(62, false)]//CancelSkill
        private void RPC_062(NetMessage message, NetMessageInfo info)
        {
            
        }
        #endregion
        #region Overridden Methods
        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (!m_owner.TryGetField(CreatureFields.Spells, out m_spells))
                throw new InvalidOperationException();
        }

        protected override void OnViewCreated()
        {
            base.OnViewCreated();
            m_view.SubscribeMarkedRpcsOnComponent(this);
        }

        protected override void OnUpdate(TimeSpan time)
        {
            base.OnUpdate(time);
            var delta = time.Milliseconds;
            if (m_gcd > 0)
                m_gcd -= delta;
            m_cast.Update(delta);
            m_cooldowns.Update(delta);
        }
        #endregion
    }
}