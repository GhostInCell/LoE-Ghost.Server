using Ghost.Server.Core;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Ghost.Server.Mgrs.Map
{
    public class ObjectsMgr
    {
        private int _currentN;
        private MapServer _server;
        private readonly int _mapID;
        private ConcurrentQueue<ushort> _released;
        private ConcurrentDictionary<uint, WorldObject> _objects;
        private ConcurrentDictionary<ushort, WorldObject> _nsObjects;
        private ConcurrentDictionary<ushort, WorldObject> _nvObjects;
        private ConcurrentDictionary<ushort, WorldObject> _plObjects;
        private ConcurrentDictionary<ushort, WorldObject> _dnObjects;

        public int MapID
        {
            get { return _mapID; }
        }

        public MapServer Server
        {
            get { return _server; }
        }

        public ObjectsMgr(MapServer server)
        {
            _server = server;
            _mapID = server.Map.Id;
            _released = new ConcurrentQueue<ushort>();
            _objects = new ConcurrentDictionary<uint, WorldObject>();
            _nsObjects = new ConcurrentDictionary<ushort, WorldObject>();
            _nvObjects = new ConcurrentDictionary<ushort, WorldObject>();
            _plObjects = new ConcurrentDictionary<ushort, WorldObject>();
            _dnObjects = new ConcurrentDictionary<ushort, WorldObject>();
            LoadObjects();
        }

        public void Reload()
        {
            foreach (var item in _objects.Where(x => !x.Value.IsPlayer))
                item.Value.Destroy();
            _currentN = 0;
            _released = new ConcurrentQueue<ushort>();
            _nsObjects.Clear();
            _nvObjects.Clear();
            _dnObjects.Clear();
            LoadObjects();
        }

        public void Destroy()
        {
            foreach (var item in _objects)
                item.Value.Destroy();
            _objects.Clear();
            _nsObjects.Clear();
            _nvObjects.Clear();
            _plObjects.Clear();
            _dnObjects.Clear();
            _server = null;
            _objects = null;
            _released = null;
            _nsObjects = null;
            _nvObjects = null;
            _plObjects = null;
            _dnObjects = null;
        }

        public async void LoadObjects()
        {
            var objects = await ServerDB.SelectAllMapObjectsAsync(_mapID);
            if (objects?.Any() ?? false)
            {
                uint guid;
                foreach (var item in objects)
                {
                    if (item.ObjectID < 0)
                        guid = CreateNSObject(item);
                    else
                        guid = CreateNVObject(item);
                    ServerLogger.Log($"{_server.Name}({_mapID}): Created {(item.ObjectID < 0 ? "NS" : "NV")} object guid {guid:X8} at {item.Position}");
                }
            }
            if (!_nsObjects.ContainsKey(0))
                ServerLogger.LogWarning($"{_server.Name}({_mapID}) default spawn point not found");
        }

        public ushort GetNewGuid()
        {
            if (!_released.TryDequeue(out var result))
                result = checked((ushort)Interlocked.Increment(ref _currentN));
            return result;
        }

        public void Add(WorldObject obj)
        {
            if (!_objects.TryAdd(obj.Guid, obj))
                ServerLogger.LogError($"{_server.Name}({_mapID}): Duplicate guid {obj.Guid}");
        }

        public void Remove(WorldObject obj)
        {
            if (!_objects.TryRemove(obj.Guid, out var value))
                ServerLogger.LogServer(_server, $"attempt to remove unregistered object guid {obj.Guid}");
            else if (!ReferenceEquals(obj, value))
                throw new InvalidOperationException();
        }

        public void ReleaseGuid(uint guid)
        {
            if (_released.Contains((ushort)guid))
                ServerLogger.LogError($"{_server.Name}({_mapID}): attempt to release already released guid {guid & 0xFFFF}");
            else
                _released.Enqueue((ushort)(guid & 0xFFFF));
        }

        public void AddView(WorldObject obj)
        {
            if (!_objects.ContainsKey(obj.Guid))
                ServerLogger.LogServer(_server, $"attempt to add unregistered object guid {obj.Guid}");
            else
            {
                if (obj.IsPlayer)
                    _plObjects[obj.SGuid] = obj;
                else if (obj.IsServer)
                    _nsObjects[obj.SGuid] = obj;
                else if (obj.IsDynamic)
                    _dnObjects[obj.SGuid] = obj;
                else
                    _nvObjects[obj.SGuid] = obj;
            }
        }

        public void RemoveView(WorldObject obj)
        {
            if (!_objects.ContainsKey(obj.Guid))
                ServerLogger.LogServer(_server, $"attempt to remove unregistered object guid {obj.Guid}");
            else
            {
                WorldObject value;
                if (obj.IsPlayer)
                    _plObjects.TryRemove(obj.SGuid, out value);
                else if (obj.IsServer)
                    _nsObjects.TryRemove(obj.SGuid, out value);
                else if (obj.IsDynamic)
                    _dnObjects.TryRemove(obj.SGuid, out value);
                else
                    _nvObjects.TryRemove(obj.SGuid, out value);
                if (!ReferenceEquals(obj, value))
                    throw new InvalidOperationException();
            }
        }

        public void Teleport(WO_Player obj, ushort id = 0)
        {
            if (_nsObjects.TryGetValue(id, out var obj01) || _nsObjects.TryGetValue(0, out obj01))
            {
                obj.Teleport(obj01.Position);
                obj.Rotation = obj01.Rotation.ToRadians();
            }
        }

        public void SetPosition(WorldObject obj, ushort id = 0)
        {
            if (_nsObjects.TryGetValue(id, out var obj01) || _nsObjects.TryGetValue(0, out obj01))
            {
                obj.Position = obj01.Position;
                obj.Rotation = obj01.Rotation.ToRadians();
            }
        }

        public bool TryGetObject(uint guid, out WorldObject obj)
        {
            return _objects.TryGetValue(guid, out obj);
        }

        public bool TryGetPlayer(ushort guid, out WorldObject obj)
        {
            return _plObjects.TryGetValue(guid, out obj);
        }

        public bool TryGetNVObject(ushort guid, out WorldObject obj)
        {
            return _nvObjects.TryGetValue(guid, out obj) || 
                _plObjects.TryGetValue(guid, out obj);
        }

        public bool TryGetCreature(ushort guid, out CreatureObject obj)
        {
            obj = null;
            if (_nvObjects.TryGetValue(guid, out var ret) || _plObjects.TryGetValue(guid, out ret))
                obj = ret as CreatureObject;
            return obj != null;
        }

        public IEnumerable<WO_MOB> GetMobsInRadius(Vector3 origin, float radius)
        {
            radius *= radius;
            return _nvObjects.Select(x => x.Value).OfType<WO_MOB>().Where(x => Vector3.DistanceSquared(x.Position, origin) <= radius);
        }

        public IEnumerable<WO_MOB> GetMobsInRadius(WorldObject origin, float radius)
        {
            if (origin == null) return Enumerable.Empty<WO_MOB>();
            radius *= radius;
            return _nvObjects.Select(x => x.Value).OfType<WO_MOB>().Where(x => Vector3.DistanceSquared(x.Position, origin.Position) <= radius);
        }

        public IEnumerable<WO_Player> GetPlayersInRadius(Vector3 origin, float radius)
        {
            radius *= radius;
            return _plObjects.Select(x => x.Value).OfType<WO_Player>().Where(x => Vector3.DistanceSquared(x.Position, origin) <= radius && !x.IsDead);
        }

        public IEnumerable<WO_Player> GetPlayersInRadius(WorldObject origin, float radius)
        {
            if (origin == null) return Enumerable.Empty<WO_Player>();
            radius *= radius;
            return _plObjects.Select(x => x.Value).OfType<WO_Player>().Where(x => Vector3.DistanceSquared(x.Position, origin.Position) <= radius && !x.IsDead);
        }

        public IEnumerable<WO_MOB> GetMobsInRadiusExcept(WorldObject origin, WorldObject except, float radius)
        {
            if (origin == null) return Enumerable.Empty<WO_MOB>();
            radius *= radius;
            return _nvObjects.Select(x => x.Value).OfType<WO_MOB>().Where(x => x != except && Vector3.DistanceSquared(x.Position, origin.Position) <= radius);
        }

        public void GetMobsInRadius(WorldObject origin, float radius, List<CreatureObject> result)
        {
            if (origin == null) return;
            radius *= radius;
            result.AddRange(_nvObjects.Select(x => x.Value).OfType<WO_MOB>().Where(x => Vector3.DistanceSquared(x.Position, origin.Position) <= radius));
        }

        public void GetPlayersInRadius(WorldObject origin, float radius, List<CreatureObject> result)
        {
            if (origin == null) return;
            radius *= radius;
            result.AddRange(_plObjects.Select(x => x.Value).OfType<WO_Player>().Where(x => !x.IsDead && Vector3.DistanceSquared(x.Position, origin.Position) <= radius));
        }

        public void GetCreaturesInRadius(WorldObject origin, float radius, List<CreatureObject> result)
        {
            if (origin == null) return;
            radius *= radius;
            result.AddRange(_plObjects.Select(x => x.Value).OfType<WO_Player>().Where(x => !x.IsDead && Vector3.DistanceSquared(x.Position, origin.Position) <= radius));
            result.AddRange(_nvObjects.Select(x => x.Value).OfType<WO_MOB>().Where(x => Vector3.DistanceSquared(x.Position, origin.Position) <= radius));
        }

        private uint CreateNSObject(DB_WorldObject obj)
        {
            if (_nsObjects.ContainsKey(obj.Guid))
                ServerLogger.LogServer(_server, $"Duplicate NS guid {obj.Guid}");
            else
            {
                switch (obj.Type)
                {
                    case 0://Spawn
                        return new WO_Spawn(obj, this).Guid;
                    case 1://Switch
                        return new WO_Switch(obj, this).Guid;
                    case 2://Spawn Pool
                        return new WO_SpawnPool(obj, this).Guid;
                    default:
                        ServerLogger.LogWarning($"{_server.Name}({_mapID}) unknow NS map object type {obj.Type} guid {obj.Guid}");
                        break;
                }
            }
            return 0;
        }

        private uint CreateNVObject(DB_WorldObject obj)
        {
            switch (obj.Type)
            {
                case 0://NPC
                    return new WO_NPC(obj, this).Guid;
                case 1://Pickup
                    return new WO_Pickup(obj, this).Guid;
                case 2://Creature
                    return new WO_MOB(obj, this).Guid;
                default:
                    ServerLogger.LogWarning($"{_server.Name}({_mapID}) unknow NV map object type {obj.Type} guid {obj.Guid}");
                    break;
            }
            return 0;
        }
    }
}