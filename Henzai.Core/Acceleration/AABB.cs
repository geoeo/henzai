using System;
using System.Numerics;
using Henzai.Core.Raytracing;
using Henzai.Core.Numerics;


namespace Henzai.Core.Acceleration
{
    public sealed class AABB : Hitable
    {
        private Vector4[] _boundingCorners;

        public AABB()
        {
            _boundingCorners = new Vector4[] {new Vector4(System.Single.MaxValue), new Vector4(System.Single.MinValue)};
        }

        public AABB(Vector4 pMin, Vector4 pMax)
        {
            _boundingCorners = new Vector4[] {pMin, pMax};
        }

        public AABB(Vector3 pMin, Vector3 pMax)
        {
            _boundingCorners = new Vector4[] {new Vector4(pMin, 1.0f), new Vector4(pMax, 1.0f)};
        }

        public Vector4 PMin => _boundingCorners[0];
        public Vector4 PMax => _boundingCorners[1];
        public float TMin() => 0.001f;
        public float TMax() => 500.0f;
        public bool IntersectionAcceptable(bool hasIntersection, float t, float dotView,  Vector4 point) => hasIntersection;
        public Vector4 NormalForSurfacePoint(Vector4 point) => Vector4.Zero;
        public bool IsObstructedBySelf(Ray ray) => false;
        public bool HasIntersection(Ray ray){
            var (hasIntersection, t) = Intersect(ray); 
            var p = ray.Origin + t*ray.Direction;
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
            //             let invDir = Vector4(1.0f / ray.Direction.X, 1.0f/ ray.Direction.Y, 1.0f / ray.Direction.Z, 0.0f)
            var invDir = new Vector4(1.0f / ray.Direction.X, 1.0f/ ray.Direction.Y, 1.0f / ray.Direction.Z, 0.0f);
            // let (isXDirNeg, isYDirNeg, isZDirNeg) = (invDir.X < 0.0f, invDir.Y < 0.0f, invDir.Z < 0.0f)
            var (isXDirNeg, isYDirNeg, isZDirNeg) = (invDir.X < 0.0f, invDir.Y < 0.0f, invDir.Z < 0.0f);
            // let gamma3 = (RaytraceGeometryUtils.gamma 3)
            var gamma3 = Numerics.Utils.Gamma(3);
            // let mutable tMin = (this.boundingCorners.[Utils.boolToInt isXDirNeg].X - ray.Origin.X) * invDir.X
            var tMin = (_boundingCorners[Utils.BoolToInt(isXDirNeg)].X - ray.Origin.X) * invDir.X;
            // let mutable tMax = (this.boundingCorners.[1 - Utils.boolToInt isXDirNeg].X - ray.Origin.X) * invDir.X
            var tMax = (_boundingCorners[1 - Utils.BoolToInt(isXDirNeg)].X - ray.Origin.X) * invDir.X;
            // let tyMin = (this.boundingCorners.[Utils.boolToInt isYDirNeg].Y - ray.Origin.Y) * invDir.Y
            var tyMin = (_boundingCorners[Utils.BoolToInt(isYDirNeg)].Y - ray.Origin.Y) * invDir.Y;
            // let mutable tyMax = (this.boundingCorners.[1 - Utils.boolToInt isYDirNeg].Y - ray.Origin.Y) * invDir.Y
            var tyMax = (_boundingCorners[1 - Utils.BoolToInt(isYDirNeg)].Y - ray.Origin.Y) * invDir.Y;
            // let tzMin = (this.boundingCorners.[Utils.boolToInt isZDirNeg].Z - ray.Origin.Z) * invDir.Z
            var tzMin = (_boundingCorners[Utils.BoolToInt(isZDirNeg)].Z - ray.Origin.Z) * invDir.Z;
            // let mutable tzMax = (this.boundingCorners.[1 - Utils.boolToInt isZDirNeg].Z - ray.Origin.Z) * invDir.Z
            var tzMax = (_boundingCorners[1 - Utils.BoolToInt(isZDirNeg)].Z - ray.Origin.Z) * invDir.Z;
            // tMax <- RaytraceGeometryUtils.robustRayBounds tMax gamma3
            tMax = Utils.RobustRayBounds(tMax, gamma3);
            // tyMax <- RaytraceGeometryUtils.robustRayBounds tyMax gamma3
            tyMax = Utils.RobustRayBounds(tyMax, gamma3);
            // tzMax <- RaytraceGeometryUtils.robustRayBounds tzMax gamma3
            tzMax = Utils.RobustRayBounds(tzMax, gamma3);
            // let passed = not (tMin > tyMax || tyMin > tMax || tMin > tzMax || tzMin > tMax)
            var passed = !(tMin > tyMax || tyMin > tMax || tMin > tzMax || tzMin > tMax);

            if(tyMin > tMin)
                tMin = tyMin;
            if(tyMax < tMax)
                tMax = tyMax;
            if(tzMin > tMin)
                tMin = tzMin;
            if(tzMax < tMax) 
                tMax = tzMax;

            return (tMin < TMax() && tMax > 0.0f && passed , tMin);
        }

    }
}