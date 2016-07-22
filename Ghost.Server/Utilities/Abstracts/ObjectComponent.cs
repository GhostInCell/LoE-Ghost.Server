namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ObjectComponent
    {
        protected WorldObject _parent;
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