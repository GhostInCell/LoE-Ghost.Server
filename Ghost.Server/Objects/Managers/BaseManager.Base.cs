using Ghost.Server.Utilities.Interfaces;
using System;

namespace Ghost.Server.Objects.Managers
{
    public abstract class BaseManager : IUpdatable
    {
        protected const int SpawnedFlag = 0x10000000;
        protected const int EnabledFlag = 0x20000000;
        protected const int DisposedFlag = 0x40000000;

        protected int m_state;

        private TimeSpan m_time;

        public bool Spawned
        {
            get
            {
                return (m_state & SpawnedFlag) != 0;
            }
            set
            {
                m_state = value ? m_state | SpawnedFlag : m_state & ~SpawnedFlag;
            }
        }

        public bool Enabled
        {
            get
            {
                return (m_state & EnabledFlag) != 0;
            }
            set
            {
                m_state = value ? m_state | EnabledFlag : m_state & ~EnabledFlag;
            }
        }

        public bool Disposed
        {
            get
            {
                return (m_state & DisposedFlag) != 0;
            }
            set
            {
                m_state = value ? m_state | DisposedFlag : m_state & ~DisposedFlag;
            }
        }

        public bool Updating
        {
            get
            {
                return (m_state & (EnabledFlag | DisposedFlag)) == EnabledFlag;
            }
        }

        public TimeSpan Interval
        {
            get; set;
        }

        public abstract BaseObject Owner
        {
            get;
        }

        public BaseManager()
        {
            Interval = TimeSpan.Zero;
        }

        public void Spawn()
        {
            OnSpawn();
            m_state |= SpawnedFlag;
            m_state |= EnabledFlag;
        }

        public void Dispose()
        {
            m_state &= ~SpawnedFlag;
            m_state &= ~EnabledFlag;
            OnDispose();
            m_state |= DisposedFlag;
        }

        public void Despawn()
        {
            m_state &= ~SpawnedFlag;
            m_state &= ~EnabledFlag;
            OnDespawn();
        }

        public void Update(TimeSpan time)
        {
            if ((m_state & (EnabledFlag | DisposedFlag)) == EnabledFlag)
            {
                TimeSpan interval = Interval, curTime = m_time.Add(time);
                if (curTime >= interval)
                {
                    if (interval != TimeSpan.Zero)
                    {
                        do
                        {
                            OnUpdate(interval);
                            curTime -= interval;
                        } while (curTime >= interval);
                    }
                    else
                    {
                        OnUpdate(curTime);
                        curTime = TimeSpan.Zero;
                    }
                }
                m_time = curTime;
            }
        }

        public abstract void Initialize(BaseObject owner);
        #region Virtual Methods
        protected virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }
        protected virtual void OnDispose() { }
        protected virtual void OnUpdate(TimeSpan time) { }
        #endregion
    }
}