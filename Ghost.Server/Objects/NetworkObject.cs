using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNetR;
using System;
using System.Numerics;

namespace Ghost.Server.Objects
{
    public abstract class NetworkObject : WorldObject
    {
        protected NetworkView m_view;

        public NetworkView View
        {
            get { return m_view; }
        }

        public NetworkObject()
            : base()
        {

        }

        protected abstract NetworkView CreateView();
        #region Events Handlers
        private void View_Destroyed()
        {
            if ((m_state & SpawnedFlag) != 0)
            {
                Dispose();
                ServerLogger.LogError($"{Guid} view suddenly destroyed");
            }
        }

        private Vector3 View_GettingRotation()
        {
            return m_rotation.ToDegrees();
        }

        private Vector3 View_GettingPosition()
        {
            return m_position;
        }

        private bool View_CheckVisibility(Player player)
        {
            //WorldObject other;
            //if (arg.Id == 0 || !m_manager.TryGet(arg, out other))
            //    return true;
            //return IsVisibleTo(other);
            return true;
        }
        #endregion
        #region Virtual Methods
        protected virtual void OnCreateView() { }

        protected virtual void OnDestroyView() { }
        #endregion
        #region Overridden Methods
        protected override void OnSpawn()
        {
            base.OnSpawn();
            m_view = CreateView();
            if (m_view != null)
            {
                m_view.Destroyed += View_Destroyed;
                m_view.GettingPosition += View_GettingPosition;
                m_view.GettingRotation += View_GettingRotation;
                m_view.CheckVisibility += View_CheckVisibility;
                foreach (var manager in GetManagers<INetworkManager>())
                    manager.UpdateView(m_view);
                OnCreateView();
            }
            else throw new InvalidOperationException();
        }

        protected override void OnDespawn()
        {
            base.OnDespawn();
            if (m_view != null)
            {
                m_view.ClearSubscriptions();
                foreach (var manager in GetManagers<INetworkManager>())
                    manager.UpdateView(null);
                OnDestroyView();
                m_view = null;
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (m_view != null)
                m_view.ClearSubscriptions();
        }
        #endregion
    }
}