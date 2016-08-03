using Ghost.Server.Core.Classes;
using PNet;
using PNetR;
using System;

namespace Ghost.Server.Objects.Managers
{
    [NetComponent(5)]
    public class SpellsManager : NetworkManager<CreatureObject>
    {
        private class CountdownList
        {
            private static readonly int[] s_empty = new int[0];
            private const int GrowingFactor = 4;

            private int m_size;
            private int[] m_keys;
            private int[] m_values;

            public CountdownList()
            {
                m_size = 0;
                m_keys = s_empty;
                m_values = s_empty;
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
                Array.Resize(ref m_keys, newLength);
                Array.Resize(ref m_values, newLength);
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
        private TargetEntry m_target;
        private CountdownList m_cooldowns;
        //private Dictionary<int, int> m_spells;

        public SpellsManager()
            : base()
        {
            m_gcd = 0;
            m_target = new TargetEntry();
            m_cooldowns = new CountdownList();
        }

        public bool CanCast(int id, int upgrade)
        {
            //int mUpgrade;
            return m_gcd <= 0 && /*m_spells.TryGetValue(id, out mUpgrade) && mUpgrade == upgrade &&*/ !m_cooldowns.Contains(id);
        }

        #region RPC Handlers
        [Rpc(61, false)]//PerformSkill
        private void RPC_061(NetMessage message, NetMessageInfo info)
        {
            m_target.OnDeserialize(message);
            if (!CanCast(m_target.SpellID, m_target.Upgrade))
                return;
        }
        [Rpc(62, false)]//CancelSkill
        private void RPC_062(NetMessage message, NetMessageInfo info)
        {

        }
        #endregion
        #region Overridden Methods
        protected override void OnViewCreated()
        {
            base.OnViewCreated();
            m_view.SubscribeMarkedRpcsOnComponent(this);
        }

        protected override void OnUpdate(TimeSpan time)
        {
            base.OnUpdate(time);
            var delta = time.Milliseconds;
            if (m_gcd > 0) m_gcd -= delta;
            m_cooldowns.Update(delta);
        }
        #endregion
    }
}