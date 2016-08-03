using Ghost.Server.Utilities;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ghost.Server.Terrain.Primitives
{
    public struct Ray : IEquatable<Ray>
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }

        public float? Intersects(Plane plane)
        {
            var den = Vector3.Dot(Direction, plane.Normal);
            if (Math.Abs(den) < 0.00001f)
                return null;

            var result = (-plane.D - Vector3.Dot(plane.Normal, Position)) / den;

            if (result < 0.0f)
            {
                if (result < -0.00001f)
                    return null;
                return 0.0f;
            }
            return result;
        }

        public float? Intersects(BoundingBox box)
        {
            const float Epsilon = 1e-6f;

            float? tMin = null, tMax = null;

            if (Math.Abs(Direction.X) < Epsilon)
            {
                if (Position.X < box.Min.X || Position.X > box.Max.X)
                    return null;
            }
            else
            {
                tMin = (box.Min.X - Position.X) / Direction.X;
                tMax = (box.Max.X - Position.X) / Direction.X;

                if (tMin > tMax)
                {
                    var temp = tMin;
                    tMin = tMax;
                    tMax = temp;
                }
            }

            if (Math.Abs(Direction.Y) < Epsilon)
            {
                if (Position.Y < box.Min.Y || Position.Y > box.Max.Y)
                    return null;
            }
            else
            {
                var tMinY = (box.Min.Y - Position.Y) / Direction.Y;
                var tMaxY = (box.Max.Y - Position.Y) / Direction.Y;

                if (tMinY > tMaxY)
                {
                    var temp = tMinY;
                    tMinY = tMaxY;
                    tMaxY = temp;
                }

                if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
                    return null;

                if (!tMin.HasValue || tMinY > tMin) tMin = tMinY;
                if (!tMax.HasValue || tMaxY < tMax) tMax = tMaxY;
            }

            if (Math.Abs(Direction.Z) < Epsilon)
            {
                if (Position.Z < box.Min.Z || Position.Z > box.Max.Z)
                    return null;
            }
            else
            {
                var tMinZ = (box.Min.Z - Position.Z) / Direction.Z;
                var tMaxZ = (box.Max.Z - Position.Z) / Direction.Z;

                if (tMinZ > tMaxZ)
                {
                    var temp = tMinZ;
                    tMinZ = tMaxZ;
                    tMaxZ = temp;
                }

                if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
                    return null;

                if (!tMin.HasValue || tMinZ > tMin) tMin = tMinZ;
                if (!tMax.HasValue || tMaxZ < tMax) tMax = tMaxZ;
            }
            if ((tMin.HasValue && tMin < 0) && tMax > 0)
                return 0;
            if (tMin < 0)
                return null;
            return tMin;
        }

        public float? Intersects(BoundingSphere sphere)
        {
            var difference = sphere.Center - Position;
            var sphereRadiusSquared = sphere.Radius * sphere.Radius;
            var differenceLengthSquared = difference.LengthSquared();
            if (differenceLengthSquared < sphereRadiusSquared)
                return 0.0f;
            var distanceAlongRay = Vector3.Dot(Direction, difference);
            if (distanceAlongRay < 0)
                return null;
            var dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;
            return (dist < 0) ? null : (float?)(distanceAlongRay - Math.Sqrt(dist));
        }

        public bool Equals(Ray other)
        {
            return Position == other.Position && Direction == other.Direction;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(Position.GetHashCode(), Direction.GetHashCode());
        }

        public override string ToString()
        {
            return $"<{Position}:{Direction}>";
        }

        public override bool Equals(object obj)
        {
            return obj is Ray ? Equals((Ray)obj) : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Ray right, Ray left)
        {
            return right.Position == left.Position && right.Direction == left.Direction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Ray right, Ray left)
        {
            return right.Position != left.Position || right.Direction != left.Direction;
        }
    }
}