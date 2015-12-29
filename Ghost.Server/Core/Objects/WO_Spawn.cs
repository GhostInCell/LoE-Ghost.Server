using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;

namespace Ghost.Server.Core.Objects
{
    public class WO_Spawn : ServerObject
    {
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDSpawn;
            }
        }
        public WO_Spawn(DB_WorldObject data, ObjectsMgr manager)
            : base(data, manager)
        { Spawn(); }
    }
}