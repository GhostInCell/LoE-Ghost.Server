using Ghost.Server.Core;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Mgrs.Map
{
    public class ObjectsMgr
    {
        private ushort _currentN;
        private MapServer _server;
        private readonly int _mapID;
        private Queue<ushort> _released;
        private Dictionary<uint, WorldObject> _objects;
        private Dictionary<ushort, WorldObject> _nsObjects;
        private Dictionary<ushort, WorldObject> _nvObjects;
        private Dictionary<ushort, WorldObject> _plObjects;
        private Dictionary<ushort, WorldObject> _dnObjects;

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
            _mapID = server.Map.ID;
            _released = new Queue<ushort>();
            _objects = new Dictionary<uint, WorldObject>();
            _nsObjects = new Dictionary<ushort, WorldObject>();
            _nvObjects = new Dictionary<ushort, WorldObject>();
            _plObjects = new Dictionary<ushort, WorldObject>();
            _dnObjects = new Dictionary<ushort, WorldObject>();
            LoadObjects();
        }

        public void Reload()
        {
            WorldObject[] _toDestroy = _objects.Values.Where(x => !x.IsPlayer).ToArray();
            foreach (var item in _toDestroy) item.Destroy();
            _currentN = 0;
            _released.Clear();
            _nsObjects.Clear();
            _nvObjects.Clear();
            _dnObjects.Clear();
            _toDestroy = null;
            LoadObjects();
        }

        public void Destroy()
        {
            WorldObject[] _toDestroy = _objects.Values.ToArray();
            foreach (var item in _toDestroy) item.Destroy();
            _objects.Clear();
            _released.Clear();
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
            _toDestroy = null;
        }

        public void LoadObjects()
        {
            List<DB_WorldObject> objects;
            if (ServerDB.SelectAllMapObjects(_mapID, out objects) && objects != null)
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
                ServerLogger.LogWarn($"{_server.Name}({_mapID}) default spawn point not found");
        }

        public ushort GetNewGuid()
        {
            lock (_released)
            {
                if (_released.Count > 0) return _released.Dequeue();
                else if (_currentN < ushort.MaxValue)
                    return ++_currentN;
                throw new Exception();
            }
        }

        public void Add(WorldObject obj)
        {
            lock (_objects)
            {
                if (_objects.ContainsKey(obj.Guid))
                    ServerLogger.LogError($"{_server.Name}({_mapID}): Duplicate guid {obj.Guid}");
                else
                    _objects[obj.Guid] = obj;
            }
        }

        public void Remove(WorldObject obj)
        {
            lock (_objects)
            {
                if (!_objects.Remove(obj.Guid))
                    ServerLogger.LogServer(_server, $"attempt to remove unregistered object guid {obj.Guid}");
            }
        }

        public void ReleaseGuid(uint guid)
        {
            lock (_released)
            {
                if (_released.Contains((ushort)guid))
                    ServerLogger.LogError($"{_server.Name}({_mapID}): attempt to release already released guid {guid & 0xFFFF}");
                else
                    _released.Enqueue((ushort)(guid & 0xFFFF));
            }
        }

        public void AddView(WorldObject obj)
        {
            lock (_objects)
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
        }

        public void RemoveView(WorldObject obj)
        {
            lock (_objects)
            {
                if (!_objects.ContainsKey(obj.Guid))
                    ServerLogger.LogServer(_server, $"attempt to remove unregistered object guid {obj.Guid}");
                else
                {
                    if (obj.IsPlayer)
                        _plObjects.Remove(obj.SGuid);
                    else if (obj.IsServer)
                        _nsObjects.Remove(obj.SGuid);
                    else if (obj.IsDynamic)
                        _dnObjects.Remove(obj.SGuid);
                    else
                        _nvObjects.Remove(obj.SGuid);
                }
            }
        }

        public void Teleport(WO_Player obj, ushort id = 0)
        {
            WorldObject obj01;
            if (_nsObjects.TryGetValue(id, out obj01) || _nsObjects.TryGetValue(0, out obj01))
            {
                obj.Teleport(obj01.Position);
                obj.Rotation = obj01.Rotation.ToRadians();
            }
        }

        public void SetPosition(WorldObject obj, ushort id = 0)
        {
            WorldObject obj01;
            if (_nsObjects.TryGetValue(id, out obj01) || _nsObjects.TryGetValue(0, out obj01))
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
            WorldObject ret; obj = null;
            if (_nvObjects.TryGetValue(guid, out ret) || _plObjects.TryGetValue(guid, out ret))
                obj = ret as CreatureObject;
            return obj != null;
        }

        public IEnumerable<WO_MOB> GetMobsInRadius(Vector3 origin, float radius)
        {
            radius *= radius;
            return _nvObjects.Values.Where(x => x is WO_MOB && Vector3.DistanceSquared(x.Position, origin) <= radius).Select(x => x as WO_MOB);
        }

        public IEnumerable<WO_MOB> GetMobsInRadius(WorldObject origin, float radius)
        {
            if (origin == null) return Enumerable.Empty<WO_MOB>();
            radius *= radius;
            return _nvObjects.Values.Where(x => x is WO_MOB && Vector3.DistanceSquared(x.Position, origin.Position) <= radius).Select(x => x as WO_MOB);
        }

        public IEnumerable<WO_Player> GetPlayersInRadius(Vector3 origin, float radius)
        {
            radius *= radius;
            return _plObjects.Values.Select(x => x as WO_Player).Where(x => Vector3.DistanceSquared(x.Position, origin) <= radius && !x.IsDead);
        }

        public IEnumerable<WO_Player> GetPlayersInRadius(WorldObject origin, float radius)
        {
            if (origin == null) return Enumerable.Empty<WO_Player>();
            radius *= radius;
            return _plObjects.Values.Select(x => x as WO_Player).Where(x => Vector3.DistanceSquared(x.Position, origin.Position) <= radius && !x.IsDead);
        }

        public IEnumerable<WO_MOB> GetMobsInRadiusExcept(WorldObject origin, WorldObject except, float radius)
        {
            if (origin == null) return Enumerable.Empty<WO_MOB>();
            radius *= radius;
            return _nvObjects.Values.Where(x => x is WO_MOB && x != except && Vector3.DistanceSquared(x.Position, origin.Position) <= radius).Select(x => x as WO_MOB);
        }

        public void GetMobsInRadius(WorldObject origin, float radius, List<CreatureObject> result)
        {
            if (origin == null) return;
            radius *= radius;
            result.AddRange(_nvObjects.Values.Where(x => x is WO_MOB && Vector3.DistanceSquared(x.Position, origin.Position) <= radius).Select(x => (CreatureObject)x));
        }

        public void GetPlayersInRadius(WorldObject origin, float radius, List<CreatureObject> result)
        {
            if (origin == null) return;
            radius *= radius;
            result.AddRange(_plObjects.Values.Select(x => (CreatureObject)x).Where(x => !x.IsDead && Vector3.DistanceSquared(x.Position, origin.Position) <= radius));
        }

        public void GetCreaturesInRadius(WorldObject origin, float radius, List<CreatureObject> result)
        {
            if (origin == null) return;
            radius *= radius;
            result.AddRange(_plObjects.Values.Select(x => (CreatureObject)x).Where(x => !x.IsDead && Vector3.DistanceSquared(x.Position, origin.Position) <= radius));
            result.AddRange(_nvObjects.Values.Where(x => x is WO_MOB && Vector3.DistanceSquared(x.Position, origin.Position) <= radius).Select(x => (CreatureObject)x));
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
                    default:
                        ServerLogger.LogWarn($"{_server.Name}({_mapID}) unknow NS map object type {obj.Type} guid {obj.Guid}");
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
                    ServerLogger.LogWarn($"{_server.Name}({_mapID}) unknow NV map object type {obj.Type} guid {obj.Guid}");
                    break;
            }
            return 0;
        }
    }
}