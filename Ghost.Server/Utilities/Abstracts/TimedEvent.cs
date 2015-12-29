using System;
using System.Threading;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class TimedEvent<T> where T : class
    {
        private static readonly object _lock = new object();
        protected T _data;
        private Timer _timer;
        private bool _canceled;
        private TimeSpan _period;
        private bool _repeatable;
        public bool Canceled
        {
            get
            {
                return _canceled;
            }
        }
        public TimeSpan Period
        {
            get
            {
                return _period;
            }
        }
        public bool Repeatable
        {
            get
            {
                return _repeatable;
            }
        }
        public T Data
        {
            get
            {
                return _data;
            }
        }
        public TimedEvent(T data, TimeSpan time, bool repeatable)
        {
            _data = data;
            _period = time;
            _repeatable = repeatable;
            _timer = new Timer(Fire, null, (int)time.TotalMilliseconds, Timeout.Infinite);
        }
        public void Destroy()
        {
            _canceled = true;
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
            _data = null;
        }
        public abstract void OnFire();
        private void Fire(object state)
        {
            if (!_canceled)
            {
                OnFire();
                lock (_lock)
                    if (_repeatable && !_canceled)
                        _timer.Change((int)_period.TotalMilliseconds, Timeout.Infinite);
                    else Destroy();
            }
        }
    }
    public abstract class TimedEvent<T1, T2> 
        where T1 : class 
        where T2 : class
    {
        private static readonly object _lock = new object();
        protected T1 _data01;
        protected T2 _data02;
        private Timer _timer;
        private bool _canceled;
        private TimeSpan _period;
        private bool _repeatable;
        public bool Canceled
        {
            get
            {
                return _canceled;
            }
        }
        public TimeSpan Period
        {
            get
            {
                return _period;
            }
        }
        public bool Repeatable
        {
            get
            {
                return _repeatable;
            }
        }
        public T1 Data01
        {
            get
            {
                return _data01;
            }
        }
        public T2 Data02
        {
            get
            {
                return _data02;
            }
        }
        public TimedEvent(T1 data01, T2 data02, TimeSpan time, bool repeatable)
        {
            _period = time;
            _data01 = data01;
            _data02 = data02;
            _repeatable = repeatable;
            _timer = new Timer(Fire, null, (int)time.TotalMilliseconds, Timeout.Infinite);
        }
        public void Destroy()
        {
            _canceled = true;
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
            _data01 = null;
            _data02 = null;
        }
        public abstract void OnFire();
        private void Fire(object state)
        {
            if (!_canceled)
            {
                OnFire();
                lock (_lock)
                    if (_repeatable && !_canceled)
                        _timer.Change((int)_period.TotalMilliseconds, Timeout.Infinite);
                    else Destroy();
            }
        }
    }
}