using Ghost.Server.Utilities.Interfaces;
using PNetR;

namespace Ghost.Server.Objects.Managers
{
    public abstract class NetworkManager<TObject> : BaseManager<TObject>, INetworkManager
        where TObject : BaseObject
    {
        protected NetworkView m_view;

        public NetworkView View
        {
            get { return m_view; }
        }

        public NetworkManager()
            : base()
        {

        }

        public void UpdateView(NetworkView view)
        {
            if (view != null)
            {
                if (m_view != null)
                    OnViewDestroyed();
                m_view = view;
                OnViewCreated();
            }
            else
            {
                OnViewDestroyed();
                m_view = null;
            }
        }

        #region Virtual Methods
        protected virtual void OnViewCreated() { }
        protected virtual void OnViewDestroyed() { }
        #endregion
    }
}