using Ghost.Server.Core;
using Ghost.Server.Core.Structs;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs
{
    public static class DataMgr
    {
        private static bool _loaded;
        private readonly static List<string> _resources;

        private readonly static Dictionary<int, DB_NPC> _npcs;
        private readonly static Dictionary<int, DB_Map> _maps;
        private readonly static Dictionary<int, DB_Item> _items;
        private readonly static Dictionary<int, DB_Loot> _loots;
        private readonly static Dictionary<int, DB_Spell> _spells;
        private readonly static Dictionary<int, DB_Dialog> _dialogs;
        private readonly static Dictionary<int, DB_Creature> _creatures;
        private readonly static Dictionary<int, Tuple<ushort, string>> _messages;
        public static string Info
        {
            get { return $"Maps {_maps.Count}; Items {_items.Count}; NPCs {_npcs.Count}; Loots {_loots.Count}; Spells {_spells.Count}; Dialogs {_dialogs.Count}; Creatures {_creatures.Count}; Resources {_resources.Count}"; }
        }
        public static bool IsLoaded
        {
            get { return _loaded; }
        }
        static DataMgr()
        {
            _loaded = true;

            _loaded &= ServerDB.SelectAllMaps(out _maps);
            _loaded &= ServerDB.SelectAllNPCs(out _npcs);
            _loaded &= ServerDB.SelectAllItems(out _items);
            _loaded &= ServerDB.SelectAllLoots(out _loots);
            _loaded &= ServerDB.SelectAllSpells(out _spells);
            _loaded &= ServerDB.SelectAllDialogs(out _dialogs);
            _loaded &= ServerDB.SelectAllMessages(out _messages);
            _loaded &= ServerDB.SelectAllResources(out _resources);
            _loaded &= ServerDB.SelectAllCreatures(out _creatures);

        }
        public static DB_Map SelectMap(int id)
        {
            return _maps[id];
        }
        public static DB_NPC SelectNPC(int id)
        {
            return _npcs[id];
        }
        public static DB_Loot SelectLoot(int id)
        {
            return _loots[id];
        }
        public static DB_Spell SelectSpell(int id)
        {
            return _spells[id];
        }
        public static DB_Item SelectItem(int id)
        {
            return _items[id];
        }
        public static string SelectResource(int id)
        {
            return (id < 0 || _resources.Count < id) ? null : _resources[id];
        }
        public static DB_Creature SelectCreature(int id)
        {
            return _creatures[id];
        }
        public static Tuple<ushort, string> SelectMessage(int id)
        {
            return _messages[id];
        }
        public static IEnumerable<DB_Map> SelectAllMaps()
        {
            foreach (var map in _maps.Values) yield return map;
        }
        public static bool Select(int id, out DB_NPC entry)
        {
            return _npcs.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Map entry)
        {
            return _maps.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Loot entry)
        {
            return _loots.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Item entry)
        {
            return _items.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Spell entry)
        {
            return _spells.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Dialog entry)
        {
            return _dialogs.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Creature entry)
        {
            return _creatures.TryGetValue(id, out entry);
        }
    }
}