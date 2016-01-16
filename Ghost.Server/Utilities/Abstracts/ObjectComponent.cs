namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ObjectComponent
    {
        protected bool _enabled;
        protected WorldObject _parent;
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        public WorldObject Parent
        {
            get
            {
                return _parent;
            }
        }
        public ObjectComponent(WorldObject parent)
        {
            _parent = parent;
            _enabled = parent.Enabled;
            parent.OnDestroy += ObjectComponent_OnDestroy;
        }
        #region Events Handlers
        private void ObjectComponent_OnDestroy()
        {
            _parent = null;
        }
        #endregion
    }
}