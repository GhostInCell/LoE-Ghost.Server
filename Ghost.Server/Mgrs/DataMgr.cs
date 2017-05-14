using Ghost.Server.Core;
using Ghost.Server.Core.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ghost.Server.Mgrs
{
    public static class DataMgr
    {
        private static readonly List<Func<Task>> s_loaders;

        private static List<string> s_resources;
        private static Dictionary<int, DB_NPC> s_npcs;
        private static Dictionary<int, DB_Map> s_maps;
        private static Dictionary<int, DB_Item> s_items;
        private static Dictionary<int, DB_Loot> s_loots;
        private static Dictionary<int, DB_Spell> s_spells;
        private static Dictionary<int, DB_Dialog> s_dialogs;
        private static Dictionary<int, DB_Creature> s_creatures;
        private static Dictionary<int, DB_Movement> s_movements;
        private static Dictionary<int, (ushort, string)> s_messages;

        public static string Info
        {
            get
            {
                return $"{Environment.NewLine}{Environment.NewLine}Maps {s_maps.Count}; Items {s_items.Count}; NPCs {s_npcs.Count}; Loots {s_loots.Count}; Spells {s_spells.Count}; Dialogs {s_dialogs.Count}; Movements {s_movements.Count}; Creatures {s_creatures.Count}; Resources {s_resources.Count}{Environment.NewLine}";
            }
        }

        static DataMgr()
        {
            s_loaders = new List<Func<Task>>
            {
                async () => s_npcs = await ServerDB.SelectAllNpcsAsync() ?? throw new Exception($"Failed to load: {nameof(s_npcs)}"),
                async () => s_maps = await ServerDB.SelectAllMapsAsync() ?? throw new Exception($"Failed to load: {nameof(s_maps)}"),
                async () => s_items = await ServerDB.SelectAllItemsAsync() ?? throw new Exception($"Failed to load: {nameof(s_items)}"),
                async () => s_loots = await ServerDB.SelectAllLootsAsync() ?? throw new Exception($"Failed to load: {nameof(s_loots)}"),
                async () => s_spells = await ServerDB.SelectAllSpellsAsync() ?? throw new Exception($"Failed to load: {nameof(s_spells)}"),
                async () => s_dialogs = await ServerDB.SelectAllDialogsAsync() ?? throw new Exception($"Failed to load: {nameof(s_dialogs)}"),
                async () => s_messages = await ServerDB.SelectAllMessagesAsync() ?? throw new Exception($"Failed to load: {nameof(s_messages)}"),
                async () => s_resources = await ServerDB.SelectAllResourcesAsync() ?? throw new Exception($"Failed to load: {nameof(s_resources)}"),
                async () => s_creatures = await ServerDB.SelectAllCreaturesAsync() ?? throw new Exception($"Failed to load: {nameof(s_creatures)}"),
                async () => s_movements = await ServerDB.SelectAllMovementsAsync() ?? throw new Exception($"Failed to load: {nameof(s_movements)}")
            };
        }

        public static async Task LoadAllAsync()
        {
            await Task.WhenAll(s_loaders.Select(x => x.Invoke()));
        }

        public static DB_Map SelectMap(int id)
        {
            return s_maps[id];
        }
        public static DB_NPC SelectNPC(int id)
        {
            return s_npcs[id];
        }
        public static DB_Loot SelectLoot(int id)
        {
            return s_loots[id];
        }
        public static DB_Spell SelectSpell(int id)
        {
            return s_spells[id];
        }
        public static DB_Item SelectItem(int id)
        {
            return s_items[id];
        }
        public static string SelectResource(int id)
        {
            return (id < 0 || s_resources.Count < id) ? null : s_resources[id];
        }
        public static DB_Creature SelectCreature(int id)
        {
            return s_creatures[id];
        }
        public static (ushort, string) SelectMessage(int id)
        {
            return s_messages[id];
        }
        public static IEnumerable<DB_Map> SelectAllMaps()
        {
            foreach (var map in s_maps.Values) yield return map;
        }
        public static bool Select(int id, out DB_NPC entry)
        {
            return s_npcs.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Map entry)
        {
            return s_maps.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Loot entry)
        {
            return s_loots.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Item entry)
        {
            return s_items.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Spell entry)
        {
            return s_spells.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Dialog entry)
        {
            return s_dialogs.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Creature entry)
        {
            return s_creatures.TryGetValue(id, out entry);
        }
        public static bool Select(int id, out DB_Movement entry)
        {
            return s_movements.TryGetValue(id, out entry);
        }
    }
}