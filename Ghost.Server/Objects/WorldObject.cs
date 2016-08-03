using Ghost.Server.Terrain;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using System.Numerics;

namespace Ghost.Server.Objects
{
    public abstract class WorldObject : BaseObject, IQuadTreeItem<WorldObject>
    {
        protected uint m_phase;
        protected Vector3 m_position;
        protected Vector3 m_rotation;
        protected QuadTreeNode<WorldObject> m_node;

        public uint Phase
        {
            get { return m_phase; }
            set { m_phase = value; }
        }

        public Vector3 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        public Vector3 Rotation
        {
            get { return m_rotation; }
            set { m_rotation = value; }
        }

        public QuadTreeNode<WorldObject> Node
        {
            get { return m_node; }
            set { m_node = value; }
        }

        public WorldObject()
            : base()
        {
            m_phase = 1;
        }

        public bool IsVisibleTo(WorldObject other)
        {
            return m_manager == other.m_manager && (m_phase & other.m_phase) != 0 && 
                Vector3.DistanceSquared(m_position, other.m_position) <= Constants.MaxVisibleDistanceSquared;
        }

        public void UpdateLocation(Vector3 position, Vector3 rotation)
        {
            m_position = position;
            m_rotation = rotation;
            m_node?.Update(this);
        }

        private void RemoveFromWorld()
        {
            if (m_node != null)
            {
                m_node.Remove(this);
                m_node = null;
            }
        }

        #region Overridden Methods
        protected override void OnSpawn()
        {
            base.OnSpawn();
            m_manager.AddToWorld(this);
        }

        protected override void OnDespawn()
        {
            base.OnDespawn();
            RemoveFromWorld();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            RemoveFromWorld();
        }
        #endregion
    }
}