using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs.Map
{
    public class DialogsMgr
    {
        MapServer _server;
        private Dictionary<ushort, DialogScript> _dialogs;
        private Dictionary<ushort, Dictionary<ushort, DialogScript>> _dClones;
        public DialogsMgr(MapServer server)
        {
            _server = server;
            _dialogs = new Dictionary<ushort, DialogScript>();
            _dClones = new Dictionary<ushort, Dictionary<ushort, DialogScript>>();
        }
        public void Destroy()
        {
            foreach (var item in _dialogs.Values) item.Destroy();
            _dialogs.Clear();
            _dialogs = null;
        }
        public DialogScript GetDialog(ushort id)
        {
            DialogScript ret; DB_Dialog data;
            if (!_dialogs.TryGetValue(id, out ret) && DataMgr.Select(id, out data))
                _dialogs[id] = ret = new DialogScript(data, _server);
            return ret;
        }
        public void RemoveClones(ushort id)
        {
            _dClones.Remove(id);
        }
        public void RemoveClone(ushort id, MapPlayer owner)
        {
            Dictionary<ushort, DialogScript> clones;
            if (_dClones.TryGetValue(owner.Player.Id, out clones))
                clones.Remove(id);
        }
        public DialogScript GetClone(ushort id, MapPlayer owner)
        {
            Dictionary<ushort, DialogScript> clones; DialogScript original, ret = null;
            if (_dialogs.TryGetValue(id, out original))
            {
                if (_dClones.TryGetValue(owner.Player.Id, out clones))
                {
                    if (!clones.TryGetValue(id, out ret))
                        ret = new DialogScript(original);
                }
                else
                    _dClones[owner.Player.Id] = new Dictionary<ushort, DialogScript>()
                    {
                        { id, ret = new DialogScript(original)}
                    };
            }
            return ret;
        }
    }
}