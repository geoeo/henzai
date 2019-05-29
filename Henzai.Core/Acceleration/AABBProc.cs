using System;
using System.Numerics;
using Henzai.Core.Raytracing;
using Henzai.Core.Numerics;


namespace Henzai.Core.Acceleration
{
    public static class AABBProc
    {
        public static Vector4 Center(AABB aabb) => ((aabb.PMin + aabb.PMax) / 2.0f);

        public static Vector4 Diagonal(AABB aabb) => aabb.PMax - aabb.PMin;

        public static AABB UnionWithPoint(AABB aabb, Vector4 p)
        {
            var newMin = new Vector4(MathF.Min(aabb.PMin.X, p.X), MathF.Min(aabb.PMin.Y, p.Y), MathF.Min(aabb.PMin.Z, p.Z), 0.0f);
            var newMax = new Vector4(MathF.Max(aabb.PMax.X, p.X), MathF.Max(aabb.PMax.Y, p.Y), MathF.Max(aabb.PMax.Z, p.Z), 0.0f);
            return new AABB(newMin, newMax);
        }

        public static AABB UnionWithAABB(AABB aabb1, AABB aabb2)
        {
            var newMin = new Vector4(MathF.Min(aabb1.PMin.X, aabb2.PMin.X), MathF.Min(aabb1.PMin.Y, aabb2.PMin.Y), MathF.Min(aabb1.PMin.Z, aabb2.PMin.Z), 0.0f);
            var newMax = new Vector4(MathF.Max(aabb1.PMax.X, aabb2.PMax.X), MathF.Max(aabb1.PMax.Y, aabb2.PMax.Y), MathF.Max(aabb1.PMax.Z, aabb2.PMax.Z), 0.0f);
            return new AABB(newMin, newMax);
        }

        public static AABB Intersect(AABB aabb1, AABB aabb2)
        {
            var newMin = new Vector4(MathF.Max(aabb1.PMin.X, aabb2.PMin.X), MathF.Max(aabb1.PMin.Y, aabb2.PMin.Y), MathF.Max(aabb1.PMin.Z, aabb2.PMin.Z), 0.0f);
            var newMax = new Vector4(MathF.Min(aabb1.PMax.X, aabb2.PMax.X), MathF.Min(aabb1.PMax.Y, aabb2.PMax.Y), MathF.Min(aabb1.PMax.Z, aabb2.PMax.Z), 0.0f);
            return new AABB(newMin, newMax);
        }


        public static bool Overlaps(AABB aabb1, AABB aabb2)
        {
            var xOverlap = aabb1.PMax.X >= aabb2.PMin.X && aabb1.PMin.X <= aabb2.PMax.X;
            var yOverlap = aabb1.PMax.Y >= aabb2.PMin.Y && aabb1.PMin.Y <= aabb2.PMax.Y;
            var zOverlap = aabb1.PMax.Z >= aabb2.PMin.Z && aabb1.PMin.Z <= aabb2.PMax.Z;
            return xOverlap && yOverlap && zOverlap;
        }

        public static bool Inside(AABB aabb, Vector4 p)
        {
            var xInside = p.X >= aabb.PMin.X && p.X <= aabb.PMax.X;
            var yInside = p.Y >= aabb.PMin.Y && p.Y <= aabb.PMax.Y;
            var zInside = p.Z >= aabb.PMin.Z && p.Z <= aabb.PMax.Z;
            return xInside && yInside && zInside;

        }

        public static bool InsideExclusive(AABB aabb, Vector4 p)
        {
            var xInside = p.X >= aabb.PMin.X && p.X < aabb.PMax.X;
            var yInside = p.Y >= aabb.PMin.Y && p.Y < aabb.PMax.Y;
            var zInside = p.Z >= aabb.PMin.Z && p.Z < aabb.PMax.Z;
            return xInside && yInside && zInside;

        }

        public static AABB Expand(AABB aabb, float delta)
        {
            var newMin = new Vector4(aabb.PMin.X - delta, aabb.PMin.Y - delta, aabb.PMin.Z - delta, 0.0f);
            var newMax = new Vector4(aabb.PMax.X + delta, aabb.PMax.Y + delta, aabb.PMax.Z + delta, 0.0f);
            return new AABB(newMin, newMax);

        }
        public static float SurfaceArea(AABB aabb)
        {
            var diagonal = Diagonal(aabb);
            return 2.0f * (diagonal.X * diagonal.Y + diagonal.X * diagonal.Z + diagonal.Y * diagonal.Z);
        }
        public static float Volume(AABB aabb)
        {
            var diagonal = Diagonal(aabb);
            return diagonal.X * diagonal.Y * diagonal.Z;
        }

        // /// Returns the index of longest axis. 
        // /// X : 0, Y : 1, Z : 2
        public static SplitAxis MaximumExtent(AABB aabb)
        {
            var diagonal = Diagonal(aabb);
            var splitAxis = SplitAxis.None;
            if (diagonal.X > diagonal.Y && diagonal.X > diagonal.Z)
                splitAxis = SplitAxis.X;
            else if (diagonal.Y > diagonal.Z)
                splitAxis = SplitAxis.Y;
            else
                splitAxis = SplitAxis.Z;
            return splitAxis;
        }
        public static Vector4 Lerp(AABB aabb, Vector4 parameterVector)
        {
            var interpX = Utils.Lerp(parameterVector.X, aabb.PMin.X, aabb.PMax.X);
            var interpY = Utils.Lerp(parameterVector.Y, aabb.PMin.Y, aabb.PMax.Y);
            var interpZ = Utils.Lerp(parameterVector.Z, aabb.PMin.Z, aabb.PMax.Z);
            return new Vector4(interpX, interpY, interpZ, 1.0f);
        }

        /// Returns a poisition between pMin (0, 0, 0) and pMax (1, 1, 1)
        public static Vector4 Offset(AABB aabb, Vector4 p)
        {
            var offset = p - aabb.PMin;
            // How can this happen??
            if (aabb.PMax.X > aabb.PMin.X)
                offset.X = offset.X / (aabb.PMax.X - aabb.PMin.X);
            if (aabb.PMax.Y > aabb.PMin.Y)
                offset.Y = offset.Y / (aabb.PMax.Y - aabb.PMin.Y);
            if (aabb.PMax.Z > aabb.PMin.Z)
                offset.Z = offset.Z / (aabb.PMax.Z - aabb.PMin.Z);
            return offset;

        }

        /// plane encode the 4 coefficients (a,b,c,d) of a plane i.e. 0 = ax + by + cz + d
        /// Assuming positive halfspace is "inside" of the plane i.e. normal pointing inwards
        /// Intersection with respect to the AABB
        /// Realtime Rendering Third Editin p. 756
        //TODO: Investigate error bound for intersection
        public static IntersectionResult PlaneIntersection(AABB aabb, ref Vector4 plane)
        {
            var intersection = IntersectionResult.Intersecting;
            var c = Center(aabb);
            var d = plane.W;
            var h = (Diagonal(aabb)) / 2.0f;
            var planeNormalAbs = Vector4.Abs(plane);
            var e = h.X * planeNormalAbs.X + h.Y * planeNormalAbs.Y + h.Z * planeNormalAbs.Z;
            var s = Numerics.Vector.InMemoryDotProduct3(ref c, ref plane) + d;

            var isOutside = s - e < 0.0f;
            var isInside = s + e > 0.0f;
            
            if (isOutside && !isInside)
                intersection = IntersectionResult.Outside;
            else if (isInside && !isOutside)
                intersection = IntersectionResult.Inside;
            return intersection;
        }
    }

}