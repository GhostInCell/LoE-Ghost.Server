using Ghost.Server.Utilities;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ghost.Server.Terrain.Primitives
{
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {     
        public Vector3 Center;
        public float Radius;

        public BoundingSphere(Vector3 center, float radius)
        {
            if (radius < 0f)
                throw new ArgumentException();
            Center = center;
            Radius = radius;
        }

        public bool Contains(Vector3 point)
        {
            return Vector3.DistanceSquared(point, Center) <= Radius * Radius;
        }

        public bool Intersects(BoundingSphere other)
        {
            var maxDistance = other.Radius + Radius;
            return Vector3.DistanceSquared(other.Center, Center) < maxDistance * maxDistance;
        }

        public bool Intersects(Ray ray, out float result)
        {
            var temp = ray.Intersects(this);
            result = temp.GetValueOrDefault();
            return temp.HasValue;
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            var distance = Vector3.Dot(plane.Normal, Center) + plane.D;
            if (distance > Radius)
                return PlaneIntersectionType.Front;
            if (distance < -Radius)
                return PlaneIntersectionType.Back;
            return PlaneIntersectionType.Intersecting;
        }

        public ContainmentType Contains(BoundingSphere other)
        {
            var distanceSquared = Vector3.DistanceSquared(Center, other.Center);
            var distance = Radius + other.Radius; ;
            if (distanceSquared > distance * distance)
                return ContainmentType.Disjoint;
            distance = other.Radius - Radius;
            if (distanceSquared <= distance * distance)
                return ContainmentType.Contains;
            return ContainmentType.Intersects;
        }

        public bool Intersects(BoundingBox other)
        {
            return Vector3.DistanceSquared(Center, Vector3.Clamp(Center, other.Min, other.Max)) <= Radius * Radius;
        }

        public ContainmentType Contains(BoundingBox other)
        {
            if (!Intersects(this))
                return ContainmentType.Disjoint;
            var radiusSquared = Radius * Radius;
            Vector3 vector;
            vector.X = Center.X - other.Min.X;
            vector.Y = Center.Y - other.Max.Y;
            vector.Z = Center.Z - other.Max.Z;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector = Center - other.Max;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector.X = Center.X - other.Max.X;
            vector.Y = Center.Y - other.Min.Y;
            vector.Z = Center.Z - other.Max.Z;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector.X = Center.X - other.Min.X;
            vector.Y = Center.Y - other.Min.Y;
            vector.Z = Center.Z - other.Max.Z;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector.X = Center.X - other.Min.X;
            vector.Y = Center.Y - other.Max.Y;
            vector.Z = Center.Z - other.Min.Z;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector.X = Center.X - other.Max.X;
            vector.Y = Center.Y - other.Max.Y;
            vector.Z = Center.Z - other.Min.Z;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector.X = Center.X - other.Max.X;
            vector.Y = Center.Y - other.Min.Y;
            vector.Z = Center.Z - other.Min.Z;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            vector = Center - other.Min;
            if (vector.LengthSquared() > radiusSquared)
                return ContainmentType.Intersects;
            return ContainmentType.Contains;
        }

        public bool Equals(BoundingSphere other)
        {
            return Radius == other.Radius && Center == other.Center;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(Center.GetHashCode(), Radius.GetHashCode());
        }

        public override string ToString()
        {
            return $"<{Center}:{Radius}>";
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingSphere ? Equals((BoundingSphere)obj) : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingSphere right, BoundingSphere left)
        {
            return right.Radius == left.Radius && right.Center == left.Center;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingSphere right, BoundingSphere left)
        {
            return right.Radius != left.Radius || right.Center != left.Center;
        }
    }
}