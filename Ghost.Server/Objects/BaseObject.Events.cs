using System;
using System.Collections.Generic;

namespace Ghost.Server.Objects
{
    public abstract class BaseArgs
    {

    }

    public delegate void BaseHandler<TArgs>(BaseObject sender, TArgs args)
        where TArgs : BaseArgs;

    public partial class BaseObject
    {
        private Dictionary<uint, object> m_handlers;

        public void UnsubscribeAll()
        {
            m_handlers.Clear();
        }

        public void Unsubscribe(uint eventId)
        {
            m_handlers.Remove(eventId);
        }

        public void Notify<T>(uint eventId, T args)
            where T : BaseArgs
        {
            object handler;
            if (m_handlers.TryGetValue(eventId, out handler))
            {
                if (handler is BaseHandler<T>)
                    ((BaseHandler<T>)handler)(this, args);
            }
        }

        public void Subscribe<T>(uint eventId, BaseHandler<T> handler)
            where T : BaseArgs
        {
            object oHandler;
            if (m_handlers.TryGetValue(eventId, out oHandler))
                m_handlers[eventId] = Delegate.Combine((Delegate)oHandler, handler);
            else
                m_handlers.Add(eventId, handler);
        }

        public void Unsubscribe<T>(uint eventId, BaseHandler<T> handler)
            where T : BaseArgs
        {
            object oHandler;
            if (m_handlers.TryGetValue(eventId, out oHandler))
            {
                oHandler = Delegate.Remove((Delegate)oHandler, handler);
                if (oHandler == null)
                    m_handlers.Remove(eventId);
                else
                    m_handlers[eventId] = oHandler;
            }
        }
    }
}