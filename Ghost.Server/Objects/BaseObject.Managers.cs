using Ghost.Server.Objects.Managers;
using Ghost.Server.Utilities;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Objects
{
    public partial class BaseObject
    {
        private int m_managers_size;
        private BaseManager[] m_managers;

        public T AddManager<T>()
            where T : BaseManager
        {
            var manager = New<T>.Create();
            ArrayEx.Add(ref m_managers, ref m_managers_size, manager);
            return manager;
        }

        public T GetManager<T>()
            where T : class
        {
            var size = m_managers_size;
            var managers = m_managers;
            for (int index = 0; index < size; index++)
            {
                var manager = managers[index];
                if (manager == null)
                    break;
                if (manager is T)
                    return manager as T;
            }
            return null;
        }

        public T RequiredManager<T>()
            where T : class
        {
            var manager = GetManager<T>();
            if (manager == null)
                throw new InvalidOperationException();
            return manager;
        }

        public void RemoveManager<T>()
            where T : class
        {
            for (int index = m_managers_size - 1; index >= 0; index--)
            {
                if (m_managers[index] is T)
                {
                    ArrayEx.RemoveAt(m_managers, ref m_managers_size, index);
                    break;
                }
            }
            ArrayEx.Trim(ref m_managers, ref m_managers_size);
        }

        public void RemoveAllManagers<T>()
            where T : class
        {
            for (int index = m_managers_size - 1; index >= 0; index--)
            {
                if (m_managers[index] is T)
                    ArrayEx.RemoveAt(m_managers, ref m_managers_size, index);
            }
            ArrayEx.Trim(ref m_managers, ref m_managers_size);
        }

        public IEnumerable<T> GetManagers<T>()
            where T : class
        {
            var size = m_managers_size;
            var managers = m_managers;
            for (int index = 0; index < size; index++)
            {
                var manager = managers[index];
                if (manager == null)
                    break;
                if (manager is T)
                    yield return manager as T;
            }
        }
    }
}