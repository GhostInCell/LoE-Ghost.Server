using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Terrain.Primitives;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_SpawnPool : ServerObject
    {
        private TimeSpan m_next;
        private BoundingBox m_bound;
        private readonly ushort MinSpawnsCount;
        private readonly ushort MaxSpawnsCount;
        private readonly ushort MinCountOrLevel;
        private readonly ushort MaxCountOrLevel;
        private List<WorldObject> m_objects;


        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDSpawnPool;
            }
        }

        public override Vector3 Rotation
        {
            get { return Vector3.Zero; }
            set { throw new NotSupportedException(); }
        }

        public WO_SpawnPool(DB_WorldObject data, ObjectsMgr manager)
            : base(data, manager)
        {
            OnUpdate += WO_SpawnPool_OnUpdate;
            OnDestroy += WO_SpawnPool_OnDestroy;
            OnDespawn += WO_SpawnPool_OnDespawn;
            MinSpawnsCount = (ushort)(data.Data02 & 0xFFFF);
            MaxSpawnsCount = (ushort)(data.Data02 >> 16);
            MinCountOrLevel = (ushort)(data.Data03 & 0xFFFF);
            MaxCountOrLevel = (ushort)(data.Data03 >> 16);
            m_objects = new List<WorldObject>(MaxSpawnsCount);
            var offset = data.Rotation / 2f;
            m_bound.Min = data.Position - offset;
            m_bound.Max = data.Position + offset;
            m_next = data.Time;
            Spawn();
        }

        private void WO_SpawnPool_OnDespawn()
        {
            foreach (var item in m_objects)
                item.Destroy();
            m_objects.Clear();
            m_objects = null;
        }

        private void WO_SpawnPool_OnDestroy()
        {
            lock (m_objects)
            {
                foreach (var item in m_objects)
                    item.Destroy();
                m_objects.Clear();
                m_objects = null;
            }
        }

        private void DoSpawn()
        {
            var posX = Constants.RND.Next((int)(m_data.Rotation.X));
            var posZ = Constants.RND.Next((int)(m_data.Rotation.Z));
            var posY = Constants.RND.Next((int)(m_bound.Max.Y - m_bound.Min.Y));

            var rotation = new Vector3(0f, Constants.RND.Next(-180, +181), 0f);
            var position = new Vector3(m_bound.Min.X + posX, m_data.Position.Y, m_bound.Min.Z + posZ);
            if (!IsSpawned)
                return;
            WorldObject wObj;
            if ((m_data.Flags & 1) != 0)
            {
                var obj = new DB_WorldObject(m_data.Map, ushort.MaxValue, Math.Abs(m_data.ObjectID), 1, 0, position, rotation, -1, 
                    Constants.RND.Next(MinCountOrLevel, MaxCountOrLevel + 1), m_data.Data01);
                wObj = new WO_Pickup(obj, Manager);

            }
            else
            {
                var obj = new DB_WorldObject(m_data.Map, ushort.MaxValue, Math.Abs(m_data.ObjectID), 1, 0, position, rotation, -1, 
                    MinCountOrLevel, MaxCountOrLevel);
                wObj = new WO_MOB(obj, Manager);

            }
            lock (m_objects)
            {
                if (IsSpawned)
                {
                    wObj.OnDestroy += WoObj_OnDestroy;
                    m_objects.Add(wObj);
                }
                else
                {
                    wObj.Destroy();
                }
            }
        }

        private void WoObj_OnDestroy()
        {
            m_objects.RemoveAll(x => !x.IsSpawned);
        }

        private void WO_SpawnPool_OnUpdate(TimeSpan obj)
        {
            if (m_objects.Count < MinSpawnsCount)
            {
                DoSpawn();
                m_next = m_data.Time;
                return;
            }
            m_next -= obj;
            if (m_next <= TimeSpan.Zero && (m_objects.Count < MaxSpawnsCount))
            {
                DoSpawn();
                m_next += m_data.Time;
            }
        }
    }
}
