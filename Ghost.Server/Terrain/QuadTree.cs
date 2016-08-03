using Ghost.Server.Terrain.Primitives;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ghost.Server.Terrain
{
    public class QuadTreeNode<T>
        where T : IQuadTreeItem<T>
    {
        private T[] m_items;
        private int m_items_size;
        private QuadTreeNode<T>[] m_childs;

        public int Count
        {
            get
            {
                var result = m_items_size;
                for (int index = 0; index < m_childs.Length; index++)
                    result += m_childs[index].Count;
                return result;
            }
        }

        public int Depth
        {
            get; private set;
        }

        public float Size
        {
            get; private set;
        }

        public bool IsRoot
        {
            get
            {
                return Parent == null;
            }
        }

        public int MaxDepth
        {
            get; private set;
        }

        public Vector3 Center
        {
            get; private set;
        }

        public BoundingBox Bound
        {
            get; private set;
        }

        public QuadTreeNode<T> Parent
        {
            get; private set;
        }

        public QuadTreeNode(Vector3 center, float size, int maxDepth)
        {
            Size = size;
            MaxDepth = maxDepth;
            m_items = ArrayEx.Empty<T>();
            Bound = CreateBBox(center, size);
            m_childs = ArrayEx.Empty<QuadTreeNode<T>>();
            Center = new Vector3(center.X, 0f, center.Z);
        }

        private QuadTreeNode(QuadTreeNode<T> parent, Vector3 center, float size)
        {
            Size = size;
            Center = center;
            Parent = parent;
            Depth = parent.Depth + 1;
            MaxDepth = parent.MaxDepth;
            m_items = ArrayEx.Empty<T>();
            Bound = CreateBBox(center, size);
            m_childs = ArrayEx.Empty<QuadTreeNode<T>>();
        }

        public bool Add(T item)
        {
            if (Bound.Contains(item.Position))
            {
                if (m_childs.Length == 0)
                {
                    if (Depth < MaxDepth)
                        Subdivide();
                    else
                    {
                        item.Node = this;
                        ArrayEx.Add(ref m_items, ref m_items_size, item);
                        return true;
                    }
                }
                for (int index = 0; index < m_childs.Length; index++)
                    if (m_childs[index].Add(item))
                        return true;
            }
            return Parent?.Add(item) ?? false;
        }

        public bool Remove(T item)
        {
            if (Bound.Contains(item.Position))
            {
                if (m_childs.Length == 0 && ArrayEx.Remove(m_items, ref m_items_size, item))
                {
                    item.Node = null;
                    return true;
                }
                for (int index = 0; index < m_childs.Length; index++)
                    if (m_childs[index].Remove(item))
                        return true;
            }
            return Parent?.Remove(item) ?? false;
        }

        public bool Update(T item)
        {
            if (ArrayEx.Contains(m_items, m_items_size, item))
            {
                if (Bound.Contains(item.Position))
                    return true;
                return ArrayEx.Remove(m_items, ref m_items_size, item) && (Parent != null ? Parent.Add(item) : false);
            }
            return Add(item);
        }

        public bool Contains(T item)
        {
            if (Bound.Contains(item.Position))
            {
                if (m_childs.Length == 0)
                    return ArrayEx.Contains(m_items, m_items_size, item);
                for (int index = 0; index < m_childs.Length; index++)
                    if (m_childs[index].Contains(item))
                        return true;
            }
            return Parent?.Contains(item) ?? false;
        }

        public void Select(BoundingSphere sphere, Predicate<T> predicate, List<T> result)
        {
            if (result == null) return;
            if (Bound.Intersects(sphere))
            {
                if (m_childs.Length == 0)
                {
                    var items = m_items;
                    for (int index = 0; index < items.Length; index++)
                    {
                        var item = items[index];
                        if (item == null)
                            break;
                        if (sphere.Contains(item.Position) && (predicate?.Invoke(item) ?? true))
                            result.Add(item);
                    }
                }
                else
                {
                    for (int index = 0; index < m_childs.Length; index++)
                        m_childs[index].Select(sphere, predicate, result);
                }
            }
            Parent?.Select(sphere, predicate, result);
        }

        public void ForEach(BoundingSphere sphere, Predicate<T> predicate, Action<T> action)
        {
            if (action == null) return;
            if (Bound.Intersects(sphere))
            {
                if (m_childs.Length == 0)
                {
                    var items = m_items;
                    for (int index = 0; index < items.Length; index++)
                    {
                        var item = items[index];
                        if (item == null)
                            break;
                        if (sphere.Contains(item.Position) && (predicate?.Invoke(item) ?? true))
                            action(item);
                    }
                }
                else
                {
                    for (int index = 0; index < m_childs.Length; index++)
                        m_childs[index].ForEach(sphere, predicate, action);
                }
            }
            Parent?.ForEach(sphere, predicate, action);
        }

        public void Select(Vector3 position, float radius, Predicate<T> predicate, List<T> result)
        {
            if (result == null) return;
            Select(new BoundingSphere(position, radius), predicate, result);
        }

        public void ForEach(Vector3 position, float radius, Predicate<T> predicate, Action<T> action)
        {
            if (action == null) return;
            ForEach(new BoundingSphere(position, radius), predicate, action);
        }

        public override string ToString()
        {
            return $"{(Parent == null ? "Root" : "Child")}[Depth {Depth}; Size {Size}; Objects {m_items.Length}]";
        }

        private void Subdivide()
        {
            if (m_childs.Length != 0) return;
            var center = Center;
            var size = Size * 0.5f;
            m_childs = new QuadTreeNode<T>[4]
            {
                   new QuadTreeNode<T>(this, center + new Vector3(+size, 0f, +size), size),
                   new QuadTreeNode<T>(this, center + new Vector3(+size, 0f, -size), size),
                   new QuadTreeNode<T>(this, center + new Vector3(-size, 0f, +size), size),
                   new QuadTreeNode<T>(this, center + new Vector3(-size, 0f, -size), size)
            };
        }

        private static BoundingBox CreateBBox(Vector3 center, float size)
        {
            var bound = default(BoundingBox);
            bound.Min.X = center.X - size;
            bound.Min.Y = float.MinValue;
            bound.Min.Z = center.Z - size;
            bound.Max.X = center.X + size;
            bound.Max.Y = float.MaxValue;
            bound.Max.Z = center.Z + size;
            return bound;
        }
    }
}