using Ghost.Server.Objects.Managers;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Objects
{
    public abstract partial class BaseObject : IUpdatable
    {
        protected const int SpawnedFlag = 0x20000000;
        protected const int EnabledFlag = 0x40000000;
        protected const int DisposedFlag = -0x80000000;

        protected int m_state;
        protected ObjectGuid m_guid;
        protected ObjectManager m_manager;

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

        public ObjectGuid Guid
        {
            get { return m_guid; }
        }

        public ObjectManager Manager
        {
            get { return m_manager; }
        }

        public BaseObject()
        {
            m_managers = ArrayEx.Empty<BaseManager>();
            m_handlers = new Dictionary<uint, object>();
        }

        public void Spawn()
        {
            if ((m_state & (SpawnedFlag | DisposedFlag)) == 0)
            {
                foreach (var item in m_managers)
                    item.Spawn();
                OnSpawn();
                m_state |= SpawnedFlag;
                m_state |= EnabledFlag;
            }
        }

        public void Despawn()
        {
            if ((m_state & (SpawnedFlag | DisposedFlag)) == SpawnedFlag)
            {
                m_state &= ~SpawnedFlag;
                m_state &= ~EnabledFlag;
                foreach (var item in m_managers)
                    item.Despawn();
                OnDespawn();
            }
        }

        public void Dispose()
        {
            m_state &= ~SpawnedFlag;
            m_state &= ~EnabledFlag;
            foreach (var item in m_managers)
                item.Dispose();
            OnDispose();
            m_state |= DisposedFlag;
        }

        public void Update(TimeSpan time)
        {
            if ((m_state & (EnabledFlag | DisposedFlag)) == EnabledFlag)
            {
                var managers = m_managers;
                for (int index = m_managers_size - 1; index >= 0; index--)
                    managers[index].Update(time);
            }
        }

        public void Initialize(ObjectGuid guid, ObjectManager manager)
        {
            m_guid = guid;
            m_manager = manager;
            if (OnLoad())
            {
                foreach (var item in m_managers)
                    item.Initialize(this);
                OnInitialize();
            }
            else throw new InvalidOperationException();
        }
        #region Virtual Methods
        protected virtual bool OnLoad()
        {
            return true;
        }
        protected virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }
        protected virtual void OnDispose() { }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate(TimeSpan time) { }
        #endregion
    }
}