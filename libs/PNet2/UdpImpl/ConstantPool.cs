#if UDP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNet.UdpImpl
{
    internal class ConstantPool<T>
    {
        private readonly Func<T> _ctor;
        private readonly Func<T, bool> _assert;
        private readonly Stack<T> _buffer = new Stack<T>();
        private readonly int _maxSize;

        public ConstantPool(Func<T> ctor, Func<T, bool> assert, int maxSize = 10)
        {
            _ctor = ctor;
            _assert = assert;
            _maxSize = maxSize;
            for (var i = 0; i < maxSize / 2; i++)
                _buffer.Push(ctor());
        }

        public T Get()
        {
            lock (_buffer)
            {
                if (_buffer.Count > 0)
                    return _buffer.Pop();
            }
            return _ctor();
        }

        public bool Recycle(T value)
        {
            if (!_assert(value)) return false;
            lock (_buffer)
            {
                if (_buffer.Count >= _maxSize) return false;
                _buffer.Push(value);
            }
            return true;
        }
    }
}
#endif