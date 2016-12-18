using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using MySql.Data.MySqlClient;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Threading;

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
        private static bool IsConnected;
        private static readonly int _maxChars;
        private static readonly string _connectionString;
        private static MySqlConnection _connection;
        public static bool Connected
        {
            get { return IsConnected; }
        }
        public static string ConnectionString
        {
            get { return _connectionString; }
        }
        static ServerDB()
        {
            var builder = new MySqlConnectionStringBuilder();
            RuntimeTypeModel.Default.Add(typeof(Vector3), false).Add("X", "Y", "Z");
            builder.Port = Configs.Get<uint>(Configs.MySQL_Port);
            builder.UserID = Configs.Get<string>(Configs.MySQL_User);
            builder.Server = Configs.Get<string>(Configs.MySQL_Host);
            builder.Database = Configs.Get<string>(Configs.MySQL_Db);
            builder.Password = Configs.Get<string>(Configs.MySQL_Pass);
            _maxChars = Configs.Get<int>(Configs.Game_MaxChars);
            _connectionString = builder.GetConnectionString(true);
            try
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.StateChange += DataBase_StateChange;
                _connection.Open();
            }
            catch { IsConnected = false; }
        }
        private static void DataBase_StateChange(object sender, StateChangeEventArgs e)
        {
            try
            {
                if (e.CurrentState == ConnectionState.Open)
                    IsConnected = true;
                else if (e.CurrentState == ConnectionState.Closed
                    && e.OriginalState == ConnectionState.Open)
                {
                    if (!_connection.Ping())
                    {
                        _connection.StateChange -= DataBase_StateChange;
                        _connection.Dispose();
                        _connection = new MySqlConnection(_connectionString);
                        _connection.StateChange += DataBase_StateChange;
                        _connection.Open();
                    }
                }
            }
            catch { IsConnected = false; }
        }
        public static bool Ping()
        {
            try
            {
                return _connection.Ping();
            }
            catch { return false; }
        }
        public static void Stop()
        {
            IsConnected = false;
            _connection.StateChange -= DataBase_StateChange;
            _connection.Close();
            _connection.Dispose();
        }
        public static bool DeleteCharacter(int id)
        {
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"DELETE FROM {tb_02} WHERE id = {id}";
                    return _cmd.ExecuteNonQuery() == 1;
                }
            }
            catch { return false; }
        }
        public static bool UpdatePony(Character entry)
        {
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"UPDATE {tb_02} SET name=@name, race=@race, gender=@gender, vdata=@vdata WHERE id=@id;";
                    _cmd.Parameters.AddWithValue("id", entry.ID);
                    _cmd.Parameters.AddWithValue("name", entry.Pony.Name);
                    _cmd.Parameters.AddWithValue("race", entry.Pony.Race);
                    _cmd.Parameters.AddWithValue("gender", entry.Pony.Gender);
                    _cmd.Parameters.AddWithValue("vdata", entry.Pony.GetBytes());
                    return _cmd.ExecuteNonQuery() == 1;
                }
            }
            catch { return false; }
        }
        public static bool UpdateCharacter(Character entry)
        {
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"UPDATE {tb_02} SET level={entry.Level}, map={entry.Map}, gdata=@gdata WHERE id={entry.ID};";
                    _cmd.Parameters.AddWithValue("gdata", entry.Data.GetBytes());
                    return _cmd.ExecuteNonQuery() == 1;
                }
            }
            catch { return false; }
        }
        public static bool SelectUser(int id, out DB_User entry)
        {
            var locked = false;
            entry = DB_User.Empty;
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT login, phash, access, session FROM {tb_01} WHERE id={id};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows && _result.Read())
                        {
                            entry.ID = id;
                            entry.Name = _result.GetString(0);
                            entry.Hash = _result.GetString(1);
                            entry.Access = (AccessLevel)_result.GetByte(2);
                            entry.SID = _result.GetNullString(3);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool UpdateUserSave(int id, UserSave entry)
        {
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"UPDATE {tb_01} SET data=@data WHERE id={id};";
                    _cmd.Parameters.AddWithValue("data", entry.GetBytes());
                    return _cmd.ExecuteNonQuery() == 1;
                }
            }
            catch { return false; }
        }
        public static bool SelectUserSave(int id, out UserSave entry)
        {
            entry = null;
            var locked = false;
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT data FROM {tb_01} WHERE id={id};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows && _result.Read())
                        {
                            entry = _result.GetProtoBuf<UserSave>(0);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectCharacter(int id, out Character entry)
        {
            entry = null;
            var locked = false;
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_02} WHERE id = {id};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows && _result.Read())
                        {
                            entry = new Character(id, _result.GetInt32(1), _result.GetInt16(2), _result.GetInt32(3), _result.GetPony(4), _result.GetProtoBuf<CharData>(8));
                            return true;
                        }
                    }
                }
                return false;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectCharacterData(int id, out CharData entry)
        {
            entry = null;
            var locked = false;
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT gdata FROM {tb_02} WHERE id = {id};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows && _result.Read())
                        {
                            entry = _result.GetProtoBuf<CharData>(0);
                            return entry != null;
                        }
                    }
                }
                return false;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool CreateUser(string login, string password, byte access = 1)
        {
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    var user = new UserSave();
                    _cmd.CommandText = $"INSERT INTO {tb_01} (login, phash, access, data) VALUES (@login, @phash, {access}, @data);";
                    _cmd.Parameters.AddWithValue("phash", StringExtension.PassHash(login, password));
                    _cmd.Parameters.AddWithValue("login", login);
                    _cmd.Parameters.AddWithValue("data", user.GetBytes());
                    user = null;
                    return _cmd.ExecuteNonQuery() == 1;
                }
            }
            catch { return false; }
        }
        public static bool CreateCharacter(int user, PonyData pony, out Character entry)
        {
            entry = null;
            var locked = false;
            if (!IsConnected) return false;
            try
            {
                Monitor.Enter(_connection, ref locked);
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    entry = new Character(pony);
                    _cmd.CommandText = $"INSERT INTO {tb_02} (user, level, race, gender, name, vdata, gdata) VALUES (@user, @level, @race, @gender, @name, @vdata, @gdata);";
                    _cmd.Parameters.AddWithValue("user", user);
                    _cmd.Parameters.AddWithValue("level", entry.Level);
                    _cmd.Parameters.AddWithValue("race", pony.Race);
                    _cmd.Parameters.AddWithValue("gender", pony.Gender);
                    _cmd.Parameters.AddWithValue("name", pony.Name);
                    _cmd.Parameters.AddWithValue("vdata", pony.GetBytes());
                    _cmd.Parameters.AddWithValue("gdata", entry.Data.GetBytes());
                    if (_cmd.ExecuteNonQuery() == 1)
                    {
                        entry.ID = (int)_cmd.LastInsertedId;
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool CreateNPC(ushort level, byte flags, ushort dialog, byte index, ushort movement, PonyData pony, out int id)
        {
            id = -1;
            var locked = false;
            if (!IsConnected) return false;
            try
            {
                Monitor.Enter(_connection, ref locked);
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"INSERT INTO {tb_05} (flags, level, dialog, `index`, movement, name, race, gender, eye, tail, hoof, mane, bodysize, hornsize, eyecolor, " +
                        "hoofcolor, bodycolor, haircolor0, haircolor1, haircolor2, cutiemark0, cutiemark1, cutiemark2) VALUES (@flags, @level, @dialog, @index, @movement, @name, " +
                        "@race, @gender, @eye, @tail, @hoof, @mane, @bodysize, @hornsize, @eyecolor, @hoofcolor, @bodycolor, @haircolor0, @haircolor1, @haircolor2, @cutiemark0, " +
                        "@cutiemark1, @cutiemark2);";
                    _cmd.Parameters.AddWithValue("level", level);
                    _cmd.Parameters.AddWithValue("flags", flags);
                    _cmd.Parameters.AddWithValue("index", index);
                    _cmd.Parameters.AddWithValue("eye", pony.Eye);
                    _cmd.Parameters.AddWithValue("dialog", dialog);
                    _cmd.Parameters.AddWithValue("mane", pony.Mane);
                    _cmd.Parameters.AddWithValue("tail", pony.Tail);
                    _cmd.Parameters.AddWithValue("hoof", pony.Hoof);
                    _cmd.Parameters.AddWithValue("name", pony.Name);
                    _cmd.Parameters.AddWithValue("race", pony.Race);
                    _cmd.Parameters.AddWithValue("movement", movement);
                    _cmd.Parameters.AddWithValue("gender", pony.Gender);
                    _cmd.Parameters.AddWithValue("bodysize", pony.BodySize);
                    _cmd.Parameters.AddWithValue("hornsize", pony.HornSize);
                    _cmd.Parameters.AddWithValue("cutiemark0", pony.CutieMark0);
                    _cmd.Parameters.AddWithValue("cutiemark1", pony.CutieMark1);
                    _cmd.Parameters.AddWithValue("cutiemark2", pony.CutieMark2);
                    _cmd.Parameters.AddWithValue("eyecolor", pony.EyeColor);
                    _cmd.Parameters.AddWithValue("hoofcolor", pony.HoofColor);
                    _cmd.Parameters.AddWithValue("bodycolor", pony.BodyColor);
                    _cmd.Parameters.AddWithValue("haircolor0", pony.HairColor0);
                    _cmd.Parameters.AddWithValue("haircolor1", pony.HairColor1);
                    _cmd.Parameters.AddWithValue("haircolor2", pony.HairColor2);
                    if (_cmd.ExecuteNonQuery() == 1)
                    {
                        id = (int)_cmd.LastInsertedId;
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool CreateObjectAt(WorldObject entry, int mapID, ushort guid, int objectID, byte type, byte flags, float time, params int[] data)
        {
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"INSERT INTO {tb_03_01} (map, guid, object, type, flags, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, time, data01, data02, data03) " +
                        $"VALUES ({mapID}, {guid}, {objectID}, {type}, {flags}, @pos_x, @pos_y, @pos_z, @rot_x, @rot_y, @rot_z, @time, @data01, @data02, @data03);";
                    _cmd.Parameters.AddWithValue("time", time);
                    var vector = entry.Position;
                    _cmd.Parameters.AddWithValue("pos_x", vector.X);
                    _cmd.Parameters.AddWithValue("pos_y", vector.Y);
                    _cmd.Parameters.AddWithValue("pos_z", vector.Z);
                    vector = entry.Rotation.ToDegrees();
                    _cmd.Parameters.AddWithValue("rot_x", vector.X);
                    _cmd.Parameters.AddWithValue("rot_y", vector.Y);
                    _cmd.Parameters.AddWithValue("rot_z", vector.Z);
                    _cmd.Parameters.AddWithValue("data01", data?.Length >= 1 ? data[0] : -1);
                    _cmd.Parameters.AddWithValue("data02", data?.Length >= 2 ? data[1] : -1);
                    _cmd.Parameters.AddWithValue("data03", data?.Length >= 3 ? data[2] : -1);
                    return _cmd.ExecuteNonQuery() == 1;
                }
            }
            catch { return false; }
        }
        public static bool SelectAllResources(out List<string> data)
        {
            var locked = false;
            data = new List<string>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT path FROM {tb_06};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows)
                            while (_result.Read())
                                data.Add(_result.GetString(0));
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllNPCs(out Dictionary<int, DB_NPC> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_NPC>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT a.*, b.item, c.* FROM {tb_05} a LEFT JOIN {tb_05_02} b ON a.id = b.id LEFT JOIN {tb_05_01} c ON a.id = c.id;";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        DB_NPC entry; int id;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetInt32(0);
                                if (data.ContainsKey(id))
                                    data[id].Items.Add(_result.GetInt32(24));
                                else
                                {
                                    entry = new DB_NPC(id, _result.GetByte(1), _result.GetInt16(2), _result.GetUInt16(3), _result.GetByte(4), _result.GetUInt16(5),
                                        _result.GetPonyOld(6));
                                    if ((entry.Flags & NPCFlags.Trader) > 0)
                                        entry.Items.Add(_result.GetInt32(24));
                                    if ((entry.Flags & NPCFlags.Wears) > 0)
                                        for (int i = 26; i <= 33; i++)
                                            entry.Wears.Add(_result.GetInt32(i));
                                    data[id] = entry;
                                }
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllMaps(out Dictionary<int, DB_Map> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Map>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_03};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        int id;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetInt32(0);
                                data[id] = new DB_Map(id, _result.GetString(1), _result.GetByte(2));
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllLoots(out Dictionary<int, DB_Loot> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Loot>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_09};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        DB_Loot entry; int id;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetInt32(0);
                                if (data.ContainsKey(id))
                                    data[id].Loot.Add(new Tuple<int, int, int, float, int, int>(_result.GetInt32(1), _result.GetInt32(2), _result.GetInt32(3),
                                        _result.GetFloat(4), _result.GetInt32(5), _result.GetInt32(6)));
                                else
                                {
                                    entry = new DB_Loot(id);
                                    entry.Loot.Add(new Tuple<int, int, int, float, int, int>(_result.GetInt32(1), _result.GetInt32(2), _result.GetInt32(3),
                                        _result.GetFloat(4), _result.GetInt32(5), _result.GetInt32(6)));
                                    data[id] = entry;
                                }
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllItems(out Dictionary<int, DB_Item> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Item>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT a.*, b.stat, b.value FROM {tb_04} a LEFT JOIN {tb_04_01} b ON a.id = b.id;";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        DB_Item entry; int id;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetInt32(0);
                                if (data.ContainsKey(id))
                                    data[id].Stats.Add(new Tuple<Stats, int>((Stats)_result.GetByte(10), _result.GetInt32(11)));
                                else
                                {
                                    entry = new DB_Item(id, _result.GetNullString(1), _result.GetByte(2), _result.GetByte(3), _result.GetByte(4), 
                                        _result.GetUInt16(5), _result.GetByte(6), _result.GetInt32(7), _result.GetInt32(8), _result.GetUInt32(9));
                                    if ((entry.Flags & ItemFlags.Stats) > 0)
                                        entry.Stats.Add(new Tuple<Stats, int>((Stats)_result.GetByte(10), _result.GetInt32(11)));
                                    data[id] = entry;
                                }
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllSpells(out Dictionary<int, DB_Spell> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Spell>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_11};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        int id; byte index; DB_Spell spell;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetInt32(0);
                                index = _result.GetByte(1);
                                if (!data.TryGetValue(id, out spell))
                                    data[id] = spell = new DB_Spell(id);
                                spell.Effects.Add(index, new DB_SpellEffect(_result.GetByte(2), _result.GetByte(3), _result.GetFloat(4), 
                                    _result.GetFloat(5), _result.GetFloat(6), _result.GetFloat(7), _result.GetFloat(8), _result.GetFloat(9)));
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllDialogs(out Dictionary<int, DB_Dialog> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Dialog>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_08};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        ushort id; short state; DB_Dialog script;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetUInt16(0);
                                state = _result.GetInt16(1);
                                if (!data.TryGetValue(id, out script)) data[id] = script = new DB_Dialog(id);
                                script.Entries.Add(state, new DialogEntry(_result.GetByte(2), _result.GetByte(3), _result.GetInt32(4), _result.GetByte(5),
                                    _result.GetInt32(6), _result.GetInt32(7), _result.GetByte(8), _result.GetInt32(9), _result.GetInt32(10)));
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllMovements(out Dictionary<int, DB_Movement> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Movement>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_12};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        ushort id; ushort state; DB_Movement script;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetUInt16(0);
                                state = _result.GetUInt16(1);
                                if (!data.TryGetValue(id, out script)) data[id] = script = new DB_Movement(id);
                                script.Entries.Add(state, new MovementEntry(_result.GetByte(2), _result.GetInt32(3), _result.GetInt32(4), _result.GetByte(5),
                                    _result.GetInt32(6), _result.GetInt32(7), _result.GetVector3(8), _result.GetVector3(11)));
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllCreatures(out Dictionary<int, DB_Creature> data)
        {
            var locked = false;
            data = new Dictionary<int, DB_Creature>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_07};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        int id;
                        if (_result.HasRows)
                            while (_result.Read())
                            {
                                id = _result.GetInt32(0);
                                data[id] = new DB_Creature(id, _result.GetInt32(1), _result.GetByte(2), _result.GetInt32(3), _result.GetFloat(4), 
                                    _result.GetInt32(5), _result.GetUInt16(6), _result.GetFloat(7), _result.GetFloat(8), _result.GetFloat(9), 
                                    _result.GetFloat(10), _result.GetFloat(11), _result.GetFloat(12), _result.GetFloat(13), _result.GetFloat(14), 
                                    _result.GetFloat(15), _result.GetFloat(16), _result.GetFloat(17));
                            }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllMapObjects(int map, out List<DB_WorldObject> data)
        {
            var locked = false;
            data = null;
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_03_01} WHERE map = {map};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows)
                        {
                            data = new List<DB_WorldObject>();
                            while (_result.Read())
                                data.Add(new DB_WorldObject(_result.GetInt32(0), _result.GetUInt16(1), _result.GetInt32(2), _result.GetByte(3), 
                                    _result.GetByte(4), _result.GetVector3(5), _result.GetVector3(8), _result.GetFloat(11), _result.GetInt32(12), 
                                    _result.GetInt32(13), _result.GetInt32(14)));
                        }
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllUserCharacters(int user, out List<Character> data)
        {
            var locked = false;
            data = new List<Character>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_02} WHERE user = {user} LIMIT {_maxChars};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows)
                            while (_result.Read())
                                data.Add(new Character(_result.GetInt32(0), _result.GetInt32(1), _result.GetInt16(2), _result.GetInt32(3), 
                                    _result.GetPony(4), _result.GetProtoBuf<CharData>(8)));
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
        public static bool SelectAllMessages(out Dictionary<int, Tuple<ushort, string>> data)
        {
            var locked = false;
            data = new Dictionary<int, Tuple<ushort, string>>();
            if (!IsConnected) return false;
            try
            {
                using (MySqlCommand _cmd = _connection.CreateCommand())
                {
                    _cmd.CommandText = $"SELECT * FROM {tb_10};";
                    Monitor.Enter(_connection, ref locked);
                    using (MySqlDataReader _result = _cmd.ExecuteReader())
                    {
                        if (_result.HasRows)
                            while (_result.Read())
                                data[_result.GetInt32(0)] = new Tuple<ushort, string>(_result.GetUInt16(1), _result.GetString(2));
                    }
                }
                return true;
            }
            catch { return false; }
            finally { if (locked) Monitor.Exit(_connection); }
        }
    }
}