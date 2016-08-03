using Ghost.Server.Utilities;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ghost.Server.Terrain.Primitives
{
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public Vector3 Min;
        public Vector3 Max;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(Vector3 vector)
        {
            if (vector.X < Min.X || vector.X > Max.X || vector.Y < Min.Y || vector.Y > Max.Y || vector.Z < Min.Z || vector.Z > Max.Z)
                return false;
            return true;
        }

        public bool Intersects(BoundingBox other)
        {
            return Max.X >= other.Min.X && Min.X <= other.Max.X && Max.Y >= other.Min.Y && Min.Y <= other.Max.Y && Max.Z >= other.Min.Z && Min.Z <= other.Max.Z;
        }

        public bool Intersects(Ray ray, out float result)
        {
            var temp = ray.Intersects(this);
            result = temp.GetValueOrDefault();
            return temp.HasValue;
        }

        public ContainmentType Contains(BoundingBox other)
        {
            if (Max.X < other.Min.X || Min.X > other.Max.X || Max.Y < other.Min.Y || Min.Y > other.Max.Y || Max.Z < other.Min.Z || Min.Z > other.Max.Z)
                return ContainmentType.Disjoint;
            if (Min.X > other.Min.X || other.Max.X > Max.X || Min.Y > other.Min.Y || other.Max.Y > Max.Y || Min.Z > other.Min.Z || other.Max.Z > Max.Z)
                return ContainmentType.Intersects;
            return ContainmentType.Contains;
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            Vector3 vector;
            vector.X = ((plane.Normal.X >= 0f) ? Min.X : Max.X);
            vector.Y = ((plane.Normal.Y >= 0f) ? Min.Y : Max.Y);
            vector.Z = ((plane.Normal.Z >= 0f) ? Min.Z : Max.Z);
            if (Vector3.Dot(plane.Normal, vector) + plane.D > 0f)
                return PlaneIntersectionType.Front;
            vector.X = ((plane.Normal.X >= 0f) ? Max.X : Min.X);
            vector.Y = ((plane.Normal.Y >= 0f) ? Max.Y : Min.Y);
            vector.Z = ((plane.Normal.Z >= 0f) ? Max.Z : Min.Z);
            if (Vector3.Dot(plane.Normal, vector) + plane.D < 0f)
                return PlaneIntersectionType.Back;
            return PlaneIntersectionType.Intersecting;
        }

        public bool Intersects(BoundingSphere other)
        {
            return Vector3.DistanceSquared(other.Center, Vector3.Clamp(other.Center, Min, Max)) <= other.Radius * other.Radius;
        }

        public ContainmentType Contains(BoundingSphere other)
        {
            var distanceSquared = Vector3.DistanceSquared(other.Center, Vector3.Clamp(other.Center, Min, Max));
            var radius = other.Radius;
            if (distanceSquared > radius * radius)
                return ContainmentType.Disjoint;
            if (Min.X + radius > other.Center.X ||
                other.Center.X > Max.X - radius ||
                Max.X - Min.X <= radius ||
                Min.Y + radius > other.Center.Y ||
                other.Center.Y > Max.Y - radius ||
                Max.Y - Min.Y <= radius ||
                Min.Z + radius > other.Center.Z ||
                other.Center.Z > Max.Z - radius ||
                Max.X - Min.X <= radius)
                return ContainmentType.Intersects;
            return ContainmentType.Contains;
        }

        public bool Equals(BoundingBox other)
        {
            return Min == other.Min && Min == other.Min;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(Min.GetHashCode(), Max.GetHashCode());
        }

        public override string ToString()
        {
            return $"<{Min}:{Max}>";
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingBox ? Equals((BoundingBox)obj) : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingBox right, BoundingBox left)
        {
            return right.Min == left.Min && right.Min == left.Min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingBox right, BoundingBox left)
        {
            return right.Min != left.Min || right.Min != left.Min;
        }
    }
}