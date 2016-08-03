using Ghost.Server.Terrain;
using System.Numerics;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface IQuadTreeItem<T>
        where T : IQuadTreeItem<T>
    {
        Vector3 Position { get; }
        QuadTreeNode<T> Node { get; set; }
    }
}