using Ghost.Server.Core;
using Ghost.Server.Core.Structs;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs
{
    public static class DataMgr
    {
        private static List<string> m_resources;
        private static Dictionary<int, DB_NPC> m_npcs;
        private static Dictionary<int, DB_Map> m_maps;
        private static Dictionary<int, DB_Item> m_items;
        private static Dictionary<int, DB_Loot> m_loots;
        private static Dictionary<int, DB_Spell> m_spells;
        private static Dictionary<int, DB_Dialog> m_dialogs;
        private static Dictionary<int, DB_Creature> m_creatures;
        private static Dictionary<int, DB_Movement> m_movements;
        private static Dictionary<int, Tuple<ushort, string>> m_messages;

        public static string Info
        {
            get
            {
                return $"{Environment.NewLine}{Environment.NewLine}Maps {m_maps.Count}; Items {m_items.Count}; NPCs {m_npcs.Count}; Loots {m_loots.Count}; Spells {m_spells.Count}; Dialogs {m_dialogs.Count}; Movements {m_movements.Count}; Creatures {m_creatures.Count}; Resources {m_resources.Count}{Environment.NewLine}";
            }
        }
        static DataMgr()
        {
            LoadAll();
        }
        public static void LoadAll()
        {
            if (!ServerDB.SelectAllMaps(out m_maps))
                throw new Exception($"Failed to load: {nameof(m_maps)}");
            if (!ServerDB.SelectAllNPCs(out m_npcs))
                throw new Exception($"Failed to load: {nameof(m_npcs)}");
            if (!ServerDB.SelectAllItems(out m_items))
                throw new Exception($"Failed to load: {nameof(m_items)}");
            if (!ServerDB.SelectAllLoots(out m_loots))
                throw new Exception($"Failed to load: {nameof(m_loots)}");
            if (!ServerDB.SelectAllSpells(out m_spells))
                throw new Exception($"Failed to load: {nameof(m_spells)}");
            if (!ServerDB.SelectAllDialogs(out m_dialogs))
                throw new Exception($"Failed to load: {nameof(m_dialogs)}");
            if (!ServerDB.SelectAllMessages(out m_messages))
                throw new Exception($"Failed to load: {nameof(m_messages)}");
            if (!ServerDB.SelectAllResources(out m_resources))
                throw new Exception($"Failed to load: {nameof(m_resources)}");
            if (!ServerDB.SelectAllCreatures(out m_creatures))
                throw new Exception($"Failed to load: {nameof(m_creatures)}");
            if (!ServerDB.SelectAllMovements(out m_movements))
                throw new Exception($"Failed to load: {nameof(m_movements)}");
        }
        public static DB_Map SelectMap(int id)
        {
            return m_maps[id];
        }
        public static DB_NPC SelectNPC(int id)
        {
            return m_npcs[id];
        }
        public static DB_Loot SelectLoot(int id)
        {
            return m_loots[id];
        }
        public static DB_Spell SelectSpell(int id)
        {
            return m_spells[id];
        }
        public static DB_Item SelectItem(int id)
        {
            return m_items[id];
        }
        public static string SelectResource(int id)
        {
            return (id < 0 || m_resources.Count < id) ? null : m_resources[id];
        }
        public static DB_Creature SelectCreature(int id)
        {
            return m_creatures[id];
        }
        public static Tuple<ushort, string> SelectMessage(int id)
        {
            return m_messages[id];
        }
        public static IEnumerable<DB_Map> SelectAllMaps()
        {
            foreach (var map in m_maps.Values) yield return map;
        }
        public static bool Select(int id, out DB_NPC entry)
        {
            return m_npcs.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Map entry)
        {
            return m_maps.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Loot entry)
        {
            return m_loots.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Item entry)
        {
            return m_items.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Spell entry)
        {
            return m_spells.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Dialog entry)
        {
            return m_dialogs.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Creature entry)
        {
            return m_creatures.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Movement entry)
        {
            return m_movements.TryGetValue(id, out entry);
        }
    }
}