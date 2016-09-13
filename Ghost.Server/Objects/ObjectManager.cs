using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNetR;

namespace Ghost.Server.Objects
{
    public class ObjectManager
    {
        public void AddToWorld(BaseObject @object)
        {
            //throw new NotImplementedException();
        }

        public NetworkedSceneObjectView CreateSceneObject(ushort guid)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(Player player, out T @object)
            where T : BaseObject
        {
            throw new NotImplementedException();
        }
    }
}