using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghost.Server.Mgrs.Map
{
    public class DialogsMgr
    {
        MapServer _server;
        private Dictionary<ushort, DialogScript> _dialogs;
        public DialogsMgr(MapServer server)
        {
            _server = server;
            _dialogs = new Dictionary<ushort, DialogScript>();
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
    }
}