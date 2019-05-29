using System;
using System.Numerics;
using Henzai.Core.Raytracing;
using Henzai.Core.Numerics;


namespace Henzai.Core.Acceleration
{
    public sealed class AABB : Hitable
    {
        private readonly Vector4[] _boundingCorners;
        //private Matrix4x4 _world;

        public AABB()
        {
            _boundingCorners = new Vector4[] { new Vector4(System.Single.MaxValue), new Vector4(System.Single.MinValue) };
            //setWorldTransform(PMin,PMax);
        }

        public AABB(Vector4 pMin, Vector4 pMax)
        {
            _boundingCorners = new Vector4[] { pMin, pMax };
            //setWorldTransform(PMin, PMax);
        }

        public AABB(Vector3 pMin, Vector3 pMax)
        {
            _boundingCorners = new Vector4[] { new Vector4(pMin, 1.0f), new Vector4(pMax, 1.0f) };
            //setWorldTransform(PMin, PMax);
        }

        public Vector4 PMin => _boundingCorners[0];
        public Vector4 PMax => _boundingCorners[1];
        public float TMin() => 0.001f;
        public float TMax() => 500.0f;
        // TODO: Maybe return a ref
        //public Matrix4x4 World() => _world;
        public bool IntersectionAcceptable(bool hasIntersection, float t, float dotView, Vector4 point) => hasIntersection;
        public Vector4 NormalForSurfacePoint(Vector4 point) => Vector4.Zero;
        public bool IsObstructedBySelf(Ray ray) => false;

        // private void setWorldTransform(Vector4 pMin, Vector4 pMax)
        // {
        //     var scale = pMax - pMin;
            
        //     _world =
        //         new Matrix4x4(
        //         scale.X, 0.0f, 0.0f, 0.0f,
        //         0.0f, scale.Y, 0.0f, 0.0f,
        //         0.0f, 0.0f, scale.Z, 0.0f,
        //         pMin.X, pMin.Y, pMax.Z, 1.0f);
        // }

        public bool HasIntersection(Ray ray)
        {
            var (hasIntersection, t) = Intersect(ray);
            var p = ray.Origin + t * ray.Direction;
            return IntersectionAcceptable(hasIntersection, t, 0.0f, p);
        }

        public Vector4 Corner(int index)
        {
            var cornerXIndex = index & 1;
            var cornerYIndex = index & 2;
            var cornerZIndex = index & 4;

            var cornerX = _boundingCorners[cornerXIndex].X;
            var cornerY = _boundingCorners[cornerYIndex].Y;
            var cornerZ = _boundingCorners[cornerZIndex].Z;

            return new Vector4(cornerX, cornerY, cornerZ, 1.0f);
        }

        // Optimized Ray Box Intersection Phyisically Based Rendering Third Edition p. 129
        public (bool, float) Intersect(Ray ray)
        {
            var invDir = new Vector4(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y, 1.0f / ray.Direction.Z, 0.0f);

            var isXDirNeg = invDir.X < 0.0f;
            var isYDirNeg = invDir.Y < 0.0f;
            var isZDirNeg = invDir.Z < 0.0f;

            var gamma3 = Numerics.Utils.Gamma(3);
            var tMin = (_boundingCorners[Utils.BoolToInt(isXDirNeg)].X - ray.Origin.X) * invDir.X;
            var tMax = (_boundingCorners[1 - Utils.BoolToInt(isXDirNeg)].X - ray.Origin.X) * invDir.X;
            var tyMin = (_boundingCorners[Utils.BoolToInt(isYDirNeg)].Y - ray.Origin.Y) * invDir.Y;
            var tyMax = (_boundingCorners[1 - Utils.BoolToInt(isYDirNeg)].Y - ray.Origin.Y) * invDir.Y;
            var tzMin = (_boundingCorners[Utils.BoolToInt(isZDirNeg)].Z - ray.Origin.Z) * invDir.Z;
            var tzMax = (_boundingCorners[1 - Utils.BoolToInt(isZDirNeg)].Z - ray.Origin.Z) * invDir.Z;

            tMax = Utils.RobustRayBounds(tMax, gamma3);
            tyMax = Utils.RobustRayBounds(tyMax, gamma3);
            tzMax = Utils.RobustRayBounds(tzMax, gamma3);
            var passed = !(tMin > tyMax || tyMin > tMax || tMin > tzMax || tzMin > tMax);

            if (tyMin > tMin)
                tMin = tyMin;
            if (tyMax < tMax)
                tMax = tyMax;
            if (tzMin > tMin)
                tMin = tzMin;
            if (tzMax < tMax)
                tMax = tzMax;

            return (tMin < TMax() && tMax > 0.0f && passed, tMin);
        }

    }
}