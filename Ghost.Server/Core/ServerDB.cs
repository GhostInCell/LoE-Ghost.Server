using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ghost.Server.Core
{
    public static class ServerDB
    {
        private const string tb_01 = "loe_user";
        private const string tb_02 = "loe_character";
        private const string tb_03 = "loe_map";
        private const string tb_03_01 = tb_03 + "_object";
        private const string tb_04 = "loe_item";
        private const string tb_04_01 = tb_04 + "_stat";
        private const string tb_05 = "loe_npc";
        private const string tb_05_01 = tb_05 + "_wear";
        private const string tb_05_02 = tb_05 + "_trade";
        private const string tb_06 = "loe_resource";
        private const string tb_07 = "loe_creature";
        private const string tb_08 = "loe_dialog";
        private const string tb_09 = "loe_loot";
        private const string tb_10 = "loe_message";
        private const string tb_11 = "loe_spell";
        private const string tb_12 = "loe_movement";
        private const string tb_13 = "loe_ban";

        private static readonly int s_maxChars;
        private static readonly string s_connectionString;

        public static string ConnectionString
        {
            get { return s_connectionString; }
        }

        static ServerDB()
        {
            var builder = new MySqlConnectionStringBuilder()
            {
                Port = Configs.Get<uint>(Configs.MySQL_Port),
                UserID = Configs.Get<string>(Configs.MySQL_User),
                Server = Configs.Get<string>(Configs.MySQL_Host),
                Database = Configs.Get<string>(Configs.MySQL_Db),
                Password = Configs.Get<string>(Configs.MySQL_Pass),
                SslMode = MySqlSslMode.None,
                Pooling = true,
                UseCompression = true,
            };
            s_maxChars = Configs.Get<int>(Configs.Game_MaxChars);
            s_connectionString = builder.ConnectionString;
        }

        private static async Task<MySqlConnection> GetConnectionAsync()
        {
            var result = new MySqlConnection(s_connectionString);
            await result.OpenAsyncEx();
            return result;
        }

        public static async Task<bool> PingAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                    return connection.Ping();
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp, "Database ping failed");
                return false;
            }
        }

        public static async Task DeleateAllOutdatedBansAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"DELETE FROM {tb_13} WHERE ban_end <= @now";
                        command.Parameters.AddWithValue("now", DateTime.Now);
                        ServerLogger.LogInfo($"Removed {await command.ExecuteNonQueryAsyncEx()} outdated ban/bans!");
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
            }
        }

        public static async Task<bool> DeleteCharacterAsync(int id)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"DELETE FROM {tb_02} WHERE id = {id}";
                        return await command.ExecuteNonQueryAsyncEx() == 1;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<bool> UpdatePonyAsync(Character entry)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE {tb_02} SET name=@name, race=@race, gender=@gender, vdata=@vdata WHERE id=@id;";
                        command.Parameters.AddWithValue("id", entry.Id);
                        command.Parameters.AddWithValue("name", entry.Pony.Name);
                        command.Parameters.AddWithValue("race", entry.Pony.Race);
                        command.Parameters.AddWithValue("gender", entry.Pony.Gender);
                        command.Parameters.AddWithValue("vdata", entry.Pony.GetBytes());
                        return await command.ExecuteNonQueryAsyncEx() == 1;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<bool> UpdateCharacterAsync(Character entry)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE {tb_02} SET level={entry.Level}, map={entry.Map}, gdata=@gdata WHERE id={entry.Id};";
                        command.Parameters.AddWithValue("gdata", entry.Data.GetBytes());
                        return await command.ExecuteNonQueryAsyncEx() == 1;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<DB_User> SelectUserAsync(int id)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT login, phash, access, session FROM {tb_01} WHERE id={id};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            if (reader.HasRows && await reader.ReadAsyncEx())
                                return new DB_User(id, reader.GetString(0), reader.GetString(1), reader.GetByte(2), reader.GetNullString(3));
                        }
                    }
                }
                return DB_User.Empty;
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return DB_User.Empty;
            }
        }

        public static async Task<Character> SelectCharacterAsync(int id)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_02} WHERE id = {id};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            if (reader.HasRows && await reader.ReadAsyncEx())
                                return new Character(id, reader.GetInt32(1), reader.GetInt16(2), reader.GetInt32(3), reader.GetPony(4), reader.GetProtoBuf<CharData>(8));
                        }
                    }
                }
                return null;
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<bool> DeleteBanAsync(int id, IPAddress ip, BanType type)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        if (ip != null)
                            command.CommandText = $"DELETE FROM {tb_13} WHERE (ban_user = {id} OR ban_ip = {ip.ToInt64()}) AND ban_type = {(byte)type}";
                        else command.CommandText = $"DELETE FROM {tb_13} WHERE ban_user = {id} AND ban_type = {(byte)type}";
                        return await command.ExecuteNonQueryAsyncEx() > 0;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<DB_Ban> SelectBanAsync(int user, IPAddress ip, BanType type, DateTime time)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        if (ip != null)
                        {
                            command.CommandText = $"SELECT * FROM {tb_13} WHERE (ban_user = {user} OR ban_ip = {ip.ToInt64()}) AND " +
                                $"ban_type = {(byte)type} AND ban_end > @now ORDER BY ban_end DESC LIMIT 1;";
                        }
                        else
                            command.CommandText = $"SELECT * FROM {tb_13} WHERE ban_user = {user} AND ban_type = {(byte)type} AND ban_end > @now ORDER BY ban_end DESC LIMIT 1;";
                        command.Parameters.AddWithValue("now", time);
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            if (reader.HasRows && await reader.ReadAsyncEx())
                                return new DB_Ban(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt64(3), 
                                    user, reader.GetDateTime(5), reader.GetDateTime(6), reader.GetByte(7));
                        }
                    }
                }
                return DB_Ban.Empty;
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return DB_Ban.Empty;
            }
        }

        public static async Task<bool> CreateBanAsync(int id, IPAddress ip, BanType type, int banBy, int time, string reason)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO {tb_13} (ban_by, reason, ban_ip, ban_user, ban_start, ban_end, ban_type) " +
                            $"VALUES ({banBy}, @reason, {ip?.ToInt64() ?? -1}, {id}, @start, @end, {(byte)type});";
                        command.Parameters.AddWithValue("reason", reason ?? string.Empty);
                        var now = DateTime.Now;
                        command.Parameters.AddWithValue("start", now);
                        command.Parameters.AddWithValue("end", time <= 0 ? DateTime.MaxValue.Date : now.AddMinutes(time));
                        return await command.ExecuteNonQueryAsyncEx() == 1;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<bool> CreateUserAsync(string login, string password, byte access = 1)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO {tb_01} (login, phash, access) VALUES (@login, @phash, {access});";
                        command.Parameters.AddWithValue("phash", StringExtension.PassHash(login, password));
                        command.Parameters.AddWithValue("login", login);
                        return await command.ExecuteNonQueryAsyncEx() == 1;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<Character> CreateCharacterAsync(int user, PonyData pony, short level = 1)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        var data = CharsMgr.CreateCharacterData(pony, level);
                        command.CommandText = $"INSERT INTO {tb_02} (user, level, race, gender, name, vdata, gdata) VALUES (@user, {level}, @race, @gender, @name, @vdata, @gdata);";
                        command.Parameters.AddWithValue("user", user);
                        command.Parameters.AddWithValue("name", pony.Name);
                        command.Parameters.AddWithValue("race", pony.Race);
                        command.Parameters.AddWithValue("gender", pony.Gender);
                        command.Parameters.AddWithValue("vdata", pony.GetBytes());
                        command.Parameters.AddWithValue("gdata", data.GetBytes());
                        if (await command.ExecuteNonQueryAsyncEx() == 1)
                            return new Character((int)command.LastInsertedId, user, level, 0, pony, data);
                    }
                }
                return null;
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<int> CreateNpcAsync(ushort level, byte flags, ushort dialog, byte index, ushort movement, PonyData pony)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO {tb_05} (flags, level, dialog, `index`, movement, name, race, gender, eye, tail, hoof, mane, bodysize, hornsize, eyecolor, " +
                            $"hoofcolor, bodycolor, haircolor0, haircolor1, haircolor2, cutiemark0, cutiemark1, cutiemark2) VALUES ({flags}, {level}, {dialog}, {index}, {movement}, " +
                            $"@name, @race, @gender, @eye, @tail, @hoof, @mane, @bodysize, @hornsize, @eyecolor, @hoofcolor, @bodycolor, @haircolor0, @haircolor1, @haircolor2, " +
                            $"@cutiemark0, @cutiemark1, @cutiemark2);";
                        command.Parameters.AddWithValue("eye", pony.Eye);
                        command.Parameters.AddWithValue("mane", pony.Mane);
                        command.Parameters.AddWithValue("tail", pony.Tail);
                        command.Parameters.AddWithValue("hoof", pony.Hoof);
                        command.Parameters.AddWithValue("name", pony.Name);
                        command.Parameters.AddWithValue("race", pony.Race);
                        command.Parameters.AddWithValue("gender", pony.Gender);
                        command.Parameters.AddWithValue("bodysize", pony.BodySize);
                        command.Parameters.AddWithValue("hornsize", pony.HornSize);
                        command.Parameters.AddWithValue("cutiemark0", pony.CutieMark0);
                        command.Parameters.AddWithValue("cutiemark1", pony.CutieMark1);
                        command.Parameters.AddWithValue("cutiemark2", pony.CutieMark2);
                        command.Parameters.AddWithValue("eyecolor", pony.EyeColor);
                        command.Parameters.AddWithValue("hoofcolor", pony.HoofColor);
                        command.Parameters.AddWithValue("bodycolor", pony.BodyColor);
                        command.Parameters.AddWithValue("haircolor0", pony.HairColor0);
                        command.Parameters.AddWithValue("haircolor1", pony.HairColor1);
                        command.Parameters.AddWithValue("haircolor2", pony.HairColor2);
                        if (await command.ExecuteNonQueryAsyncEx() == 1)
                            return (int)command.LastInsertedId;
                    }
                }
                return -1;
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return -1;
            }
        }

        public static async Task<bool> CreateObjectAtAsync(WorldObject entry, int map, ushort guid, int objectID, byte type, byte flags, float time, params int[] data)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO {tb_03_01} (map, guid, object, type, flags, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, time, data01, data02, data03) " +
                            $"VALUES ({map}, {guid}, {objectID}, {type}, {flags}, @pos_x, @pos_y, @pos_z, @rot_x, @rot_y, @rot_z, @time, @data01, @data02, @data03);";
                        command.Parameters.AddWithValue("time", time);
                        var vector = entry.Position;
                        command.Parameters.AddWithValue("pos_x", vector.X);
                        command.Parameters.AddWithValue("pos_y", vector.Y);
                        command.Parameters.AddWithValue("pos_z", vector.Z);
                        vector = entry.Rotation.ToDegrees();
                        command.Parameters.AddWithValue("rot_x", vector.X);
                        command.Parameters.AddWithValue("rot_y", vector.Y);
                        command.Parameters.AddWithValue("rot_z", vector.Z);
                        command.Parameters.AddWithValue("data01", data?.Length >= 1 ? data[0] : -1);
                        command.Parameters.AddWithValue("data02", data?.Length >= 2 ? data[1] : -1);
                        command.Parameters.AddWithValue("data03", data?.Length >= 3 ? data[2] : -1);
                        return await command.ExecuteNonQueryAsyncEx() == 1;
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return false;
            }
        }

        public static async Task<List<string>> SelectAllResourcesAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT path FROM {tb_06};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new List<string>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                    result.Add(reader.GetString(0));
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_NPC>> SelectAllNpcsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
 
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT a.*, b.item, c.* FROM {tb_05} a LEFT JOIN {tb_05_02} b ON a.id = b.id LEFT JOIN {tb_05_01} c ON a.id = c.id;";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_NPC>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetInt32(0);
                                    if (result.TryGetValue(id, out var entry))
                                        entry.Items.Add(reader.GetInt32(24));
                                    else
                                    {
                                        entry = new DB_NPC(id, reader.GetByte(1), reader.GetInt16(2), reader.GetUInt16(3), reader.GetByte(4), reader.GetUInt16(5), reader.GetPonyOld(6));
                                        if ((entry.Flags & NPCFlags.Trader) > 0)
                                            entry.Items.Add(reader.GetInt32(24));
                                        if ((entry.Flags & NPCFlags.Wears) > 0)
                                            for (int i = 26; i <= 33; i++)
                                                entry.Wears.Add(reader.GetInt32(i));
                                        result[id] = entry;
                                    }
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Map>> SelectAllMapsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_03};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Map>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetInt32(0);
                                    result[id] = new DB_Map(id, reader.GetString(1), reader.GetByte(2));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Item>> SelectAllItemsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT a.*, b.stat, b.value FROM {tb_04} a LEFT JOIN {tb_04_01} b ON a.id = b.id;";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Item>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetInt32(0);
                                    if (result.TryGetValue(id, out var entry))
                                        entry.Stats.Add(((Stats)reader.GetByte(10), reader.GetInt32(11)));
                                    else
                                    {
                                        entry = new DB_Item(id, reader.GetNullString(1), reader.GetByte(2), reader.GetByte(3), reader.GetByte(4),
                                            reader.GetUInt16(5), reader.GetByte(6), reader.GetInt32(7), reader.GetUInt32(8), reader.GetUInt32(9));
                                        if ((entry.Flags & ItemFlags.Stats) > 0)
                                            entry.Stats.Add(((Stats)reader.GetByte(10), reader.GetInt32(11)));
                                        result[id] = entry;
                                    }
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Loot>> SelectAllLootsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_09};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Loot>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetInt32(0);
                                    if (!result.TryGetValue(id, out var entry))
                                        result[id] = entry = new DB_Loot(id);
                                    entry.Loot.Add((reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetFloat(4), reader.GetInt32(5), reader.GetInt32(6)));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Spell>> SelectAllSpellsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_11};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Spell>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetInt32(0);
                                    var index = reader.GetByte(1);
                                    if (!result.TryGetValue(id, out var entry))
                                        result[id] = entry = new DB_Spell(id);
                                    entry.Effects.Add(index, new DB_SpellEffect(reader.GetByte(2), reader.GetByte(3), reader.GetFloat(4),
                                        reader.GetFloat(5), reader.GetFloat(6), reader.GetFloat(7), reader.GetFloat(8), reader.GetFloat(9)));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Dialog>> SelectAllDialogsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_08};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Dialog>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetUInt16(0);
                                    var state = reader.GetInt16(1);
                                    if (!result.TryGetValue(id, out var entry))
                                        result[id] = entry = new DB_Dialog(id);
                                    entry.Entries.Add(state, new DialogEntry(reader.GetByte(2), reader.GetByte(3), reader.GetInt32(4), reader.GetByte(5),
                                        reader.GetInt32(6), reader.GetInt32(7), reader.GetByte(8), reader.GetInt32(9), reader.GetInt32(10)));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Movement>> SelectAllMovementsAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_12};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Movement>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetUInt16(0);
                                    var state = reader.GetUInt16(1);
                                    if (!result.TryGetValue(id, out var entry))
                                        result[id] = entry = new DB_Movement(id);
                                    entry.Entries.Add(state, new MovementEntry(reader.GetByte(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetByte(5),
                                        reader.GetInt32(6), reader.GetInt32(7), reader.GetVector3(8), reader.GetVector3(11)));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, DB_Creature>> SelectAllCreaturesAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_07};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, DB_Creature>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    var id = reader.GetInt32(0);
                                    result[id] = new DB_Creature(id, reader.GetInt32(1), reader.GetByte(2), reader.GetInt32(3), reader.GetFloat(4), reader.GetInt32(5), 
                                        reader.GetUInt16(6), reader.GetFloat(7), reader.GetFloat(8), reader.GetFloat(9), reader.GetFloat(10), reader.GetFloat(11), 
                                        reader.GetFloat(12), reader.GetFloat(13), reader.GetFloat(14), reader.GetFloat(15), reader.GetFloat(16), reader.GetFloat(17));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<Dictionary<int, (ushort, string)>> SelectAllMessagesAsync()
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_10};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new Dictionary<int, (ushort, string)>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                    result[reader.GetInt32(0)] = (reader.GetUInt16(1), reader.GetString(2));
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<List<DB_WorldObject>> SelectAllMapObjectsAsync(int map)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_03_01} WHERE map = {map};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new List<DB_WorldObject>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    result.Add(new DB_WorldObject(reader.GetInt32(0), reader.GetUInt16(1), reader.GetInt32(2), reader.GetByte(3), reader.GetByte(4), 
                                        reader.GetVector3(5), reader.GetVector3(8), reader.GetFloat(11), reader.GetInt32(12), reader.GetInt32(13), reader.GetInt32(14)));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }

        public static async Task<List<Character>> SelectAllUserCharactersAsync(int user)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tb_02} WHERE user = {user} LIMIT {s_maxChars};";
                        using (var reader = await command.ExecuteReaderAsyncEx())
                        {
                            var result = new List<Character>();
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsyncEx())
                                {
                                    result.Add(new Character(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt16(2), reader.GetInt32(3),
                                        reader.GetPony(4), reader.GetProtoBuf<CharData>(8)));
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                ServerLogger.LogException(exp);
                return null;
            }
        }
    }
}