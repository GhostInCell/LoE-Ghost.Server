using Ghost.Server.Core;
using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ghost.Server.Mgrs
{
    public static class CharsMgr
    {
        public static readonly float MinHornSize = 0.001f;
        public static readonly float MaxHornSize = 5.000f;
        public static readonly float MinBodySize = 0.001f;
        public static readonly float MaxBodySize = 5.000f;

        public static readonly int MaxChars;
        public static readonly short MaxLevel;
        public static readonly short TalentPointsPerLevel;

        public static readonly string NamesFilter;

        private static readonly List<Regex> s_regex = new List<Regex>();
        private static readonly List<string> s_words = new List<string>();
        private static readonly List<string> s_exact = new List<string>();

        static CharsMgr()
        {

            MaxChars = Configs.Get<int>(Configs.Game_MaxChars);
            MaxLevel = Configs.Get<short>(Configs.Game_MaxLevel);
            NamesFilter = Configs.Get<string>(Configs.Game_NamesFilter);
            TalentPointsPerLevel = Configs.Get<short>(Configs.Game_TalentPointsPerLevel);
            LoadNamesFilter();
        }

        public static bool CheckName(string name)
        {
            var value = name.Trim().ToLowerInvariant();
            if (s_exact.Contains(value) || s_regex.Any(x => x.IsMatch(value)))
                return false;
            foreach (var item in value.Split(' ').Select(x => x.Trim()))
            {
                if (s_words.Contains(item) || s_regex.Any(x => x.IsMatch(item)))
                    return false;
            }
            return true;
        }

        public static uint GetExpForLevel(int level)
        {
            if (level <= 0)
                return 0u;
            if (level >= MaxLevel)
                return (uint)(MaxLevel * 500 + (MaxLevel - 1) * 500);
            else
                return (uint)(level * 500 + (level - 1) * 500);
        }

        public static void ValidatePonyData(PonyData pony)
        {
            {
                var value = MathHelper.Clamp(pony.HornSize, MinHornSize, MaxHornSize);
                if (value != pony.HornSize)
                {
                    ServerLogger.LogWarn($"Resetting character \"{pony.Name}\" horn size from {pony.HornSize} to {value}");
                    pony.HornSize = value;
                }
            }
            {
                var value = MathHelper.Clamp(pony.BodySize, MinBodySize, MaxBodySize);
                if (value != pony.BodySize)
                {
                    ServerLogger.LogWarn($"Resetting character \"{pony.Name}\" body size from {pony.BodySize} to {value}");
                    pony.BodySize = value;
                }
            }
            {
                var value = (Gender)MathHelper.Clamp((byte)pony.Gender, (byte)Gender.Mare, (byte)Gender.Stallion);
                if (value != pony.Gender)
                {
                    ServerLogger.LogWarn($"Resetting character \"{pony.Name}\" gender from {pony.Gender} to {value}");
                    pony.Gender = value;
                }
            }
            {
                var value = (CharacterType)MathHelper.Clamp((byte)pony.Race, (byte)CharacterType.EarthPony, (byte)CharacterType.Pegasus);
                if (value != pony.Race)
                {
                    ServerLogger.LogWarn($"Resetting character \"{pony.Name}\" race from {pony.Race} to {value}");
                    pony.Race = value;
                }
            }
        }

        public static async void SaveCharacter(Character entry)
        {
            if (!await ServerDB.UpdateCharacterAsync(entry))
                ServerLogger.LogError($"Couldn't save character {entry.Id}");
        }

        public static async Task<bool> SaveCharacterAsync(Character entry)
        {
            if (await ServerDB.UpdateCharacterAsync(entry))
                return true;
            ServerLogger.LogError($"Couldn't save character {entry.Id}");
            return false;
        }

        public static CharData CreateCharacterData(PonyData pony, short level)
        {
            var data = new CharData()
            {
                Bits = 15 * level
            };

            data.Skills[10] = 0;//Ground Pound
            data.Skills[44] = 0;//Bubble Barrage

            data.Talents[TalentMarkId.Foal] = new TalentData(level);
            data.Talents[TalentMarkId.Music] = new TalentData(level);
            data.Talents[TalentMarkId.Magic] = new TalentData(level);
            data.Talents[TalentMarkId.Animal] = new TalentData(level);
            data.Talents[TalentMarkId.Flying] = new TalentData(level);
            data.Talents[TalentMarkId.Combat] = new TalentData(level);
            data.Talents[TalentMarkId.Artisan] = new TalentData(level);
            data.Talents[TalentMarkId.Medical] = new TalentData(level);
            data.Talents[TalentMarkId.Partying] = new TalentData(level);

            switch (pony.Race)
            {
                case CharacterType.EarthPony:
                    data.Skills[5] = 0;//Seismic Buck
                    data.Skills[16] = 0;//Rough Terrain
                    data.Skills[21] = 0;//Pillow Barrage
                    data.InventorySlots = 38;
                    break;
                case CharacterType.Unicorn:
                    data.Skills[2] = 0;//Teleport
                    data.Skills[9] = 0;//Rainbow Fields
                    data.Skills[15] = 0;//Magical Arrow
                    data.Skills[31] = 0;//Sphere of Protection
                    data.InventorySlots = 32;
                    break;
                case CharacterType.Pegasus:
                    data.Skills[11] = 0;//Dual Cyclone
                    data.Skills[14] = 0;//Gale
                    data.InventorySlots = 32;
                    break;
            }
            return data;
        }

        private static void LoadNamesFilter()
        {
            try
            {
                if (File.Exists(NamesFilter))
                {
                    foreach (var item01 in File.ReadLines(NamesFilter, Encoding.UTF8))
                    {
                        if (string.IsNullOrWhiteSpace(item01) || item01 == string.Empty || item01[0] == '#')
                            continue;
                        if (item01[0] == '!' && item01.Length > 1)
                            s_words.AddRange(item01.Substring(1).ToLowerInvariant().Split(' ').Select(x => x.Trim()));
                        else if (item01[0] == '@' && item01.Length > 1)
                            s_regex.Add(new Regex(item01.Trim().Substring(1), RegexOptions.Compiled | RegexOptions.IgnoreCase));
                        else s_exact.Add(item01.Trim().ToLowerInvariant());
                    }
                }
                ServerLogger.LogInfo($"Loaded {s_exact.Count + s_words.Count + s_regex.Count} naming rule(s)");
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp, $"Exception while loading names filter \"{NamesFilter}\"");
            }
        }
    }
}