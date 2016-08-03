using Ghost.Server.Objects.Managers;

namespace Ghost.Server.Objects
{
    public abstract class MovableObject : NetworkObject
    {
        protected MotionManager m_motion;

        public MotionManager Motion
        {
            get
            {
                return m_motion;
            }
        }

        public MovableObject()
            : base()
        {
            m_motion = AddManager<MotionManager>();
        }
    }
}