using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs.Map;
using System;
using System.Numerics;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class ServerObject : WorldObject
    {
        protected readonly DB_WorldObject _data;
        public DB_WorldObject Data
        {
            get
            {
                return _data;
            }
        }
        public override ushort SGuid
        {
            get
            {
                return _data.Guid;
            }
        }
        public override Vector3 Position
        {
            get { return _data.Position; }
            set { throw new NotSupportedException(); }
        }
        public override Vector3 Rotation
        {
            get { return _data.Rotation; }
            set { throw new NotSupportedException(); }
        }
        public ServerObject(DB_WorldObject data, ObjectsMgr manager)
            : base(data.Guid | Constants.ServerObject, manager)
        {
            _data = data;
        }
    }
}