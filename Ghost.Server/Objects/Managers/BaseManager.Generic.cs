using System;

namespace Ghost.Server.Objects.Managers
{
    public abstract class BaseManager<TObject> : BaseManager
        where TObject : BaseObject
    {
        protected TObject m_owner;

        public override BaseObject Owner
        {
            get { return m_owner; }
        }

        public BaseManager()
            : base()
        {

        }

        public override void Initialize(BaseObject owner)
        {
            if (owner is TObject)
            {
                m_owner = (TObject)owner;
                OnInitialize();
            }
            else throw new InvalidOperationException();
        }

        #region Virtual Methods
        protected virtual void OnInitialize() { }
        #endregion
    }
}