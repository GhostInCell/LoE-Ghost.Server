using System;
using System.Collections.Concurrent;

namespace Ghost.Server.Core.Classes
{
    public class ObjectPool<T>
    {
        private int m_capacity;
        private Func<T> m_generator;
        private Action<T> m_cleaner;
        private ConcurrentQueue<T> m_pool;

        public ObjectPool(int capacity, Func<T> generator, Action<T> cleaner = null)
        {
            m_cleaner = cleaner;
            m_capacity = capacity;
            m_generator = generator ?? throw new ArgumentNullException(nameof(generator));
            m_pool = new ConcurrentQueue<T>();
            while (capacity > 0)
            {
                m_pool.Enqueue(generator());
                capacity--;
            }
        }

        public T Pop()
        {
            if (m_pool.TryDequeue(out var item))
                return item;
            return m_generator();
        }

        public void Push(ref T item)
        {
            m_cleaner?.Invoke(item);
            m_pool.Enqueue(item);
            item = default(T);
        }
    }
}