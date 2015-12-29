using Ghost.Server.Core;
using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Mgrs
{
    public static class CharsMgr
    {
        public static readonly int MaxChars;
        public static readonly short MaxLevel;
        private static readonly Dictionary<int, Character> _chars;
        static CharsMgr()
        {
            _chars = new Dictionary<int, Character>();
            MaxChars = Configs.Get<int>(Configs.Game_MaxChars);
            MaxLevel = Configs.Get<short>(Configs.Game_MaxLevel);
        }
        public static bool DeleteCharacter(int id)
        {
            lock (_chars)
            {
                if (!ServerDB.DeleteCharacter(id))
                    return false;
                _chars.Remove(id);
                return true;
            }
        }
        public static void RemoveCharacter(int id)
        {
            lock (_chars) _chars.Remove(id);
        }
        public static bool SaveCharacter(Character entry)
        {
            if (ServerDB.UpdateCharacter(entry))
                return true;
            ServerLogger.LogError($"Couldn't save character {entry.ID}");
            return false;
        }
        public static void CreateCharacterData(Character entry)
        {
            var data = new CharData();
            data.Bits = 15;
            data.Skills[10] = 0;//Ground Pound
            data.Talents[(int)Talent.Combat] = new Tuple<uint, short>(0, entry.Level);
            switch (entry.Pony.Race)
            {
                case 1:
                    data.Skills[5] = 0;//Seismic Buck
                    data.Skills[16] = 0;//Rough Terrain
                    data.Skills[21] = 0;//Pillow Barrage
                    data.InvSlots = 25;
                    break;
                case 2:
                    data.Skills[2] = 0;//Teleport
                    data.Skills[9] = 0;//Rainbow Fields
                    data.Skills[12] = 0;//Heal player
                    data.Skills[15] = 0;//Magical Arrow
                    data.InvSlots = 21;
                    break;
                case 3:
                    data.Skills[11] = 0;//Dual Cyclone
                    data.Skills[14] = 0;//Gale
                    data.InvSlots = 21;
                    break;
            }
            entry.Data = data;
        }
        public static bool SelectCharacter(int id, out Character entry)
        {
            lock (_chars)
            {
                if (_chars.TryGetValue(id, out entry))
                    return true;
                else if (ServerDB.SelectCharacter(id, out entry))
                    _chars[id] = entry;
                else return false;
                return true;
            }
        }
        public static void RemoveCharacters(IEnumerable<Character> chars)
        {
            lock (_chars) foreach (var item in chars)
                    _chars.Remove(item.ID);
        }
        public static bool SelectAllUserCharacters(int user, out List<Character> data)
        {
            lock (_chars)
               if (_chars.Any(x => x.Value.User == user))
                {
                    data = _chars.Values.Where(x => x.User == user).ToList();
                    return true;
                }
            if (ServerDB.SelectAllUserCharacters(user, out data))
            {
                lock (_chars) foreach (var item in data) _chars[item.ID] = item;
                return true;
            }
            return false;
        }
        public static bool CreateCharacter(int user, PonyData pony, out Character entry)
        {
            if (!ServerDB.CreateCharacter(user, pony, out entry))
                return false;
            _chars[entry.ID] = entry;
            return true;
        }
    }
}