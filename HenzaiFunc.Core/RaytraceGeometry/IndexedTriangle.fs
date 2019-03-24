﻿namespace HenzaiFunc.Core.RaytraceGeometry

open System
open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Numerics
open Henzai.Core.VertexGeometry

// Baldwin-Weber ray-triangle intersection algorithm: http://jcgt.org/published/0005/03/03/
/// Vertices are CCW
type IndexedTriangle<'T when 'T : struct and 'T :> VertexLocateable>(i0 : int, i1 : int, i2 : int, triangles : 'T[]) =
    inherit RaytracingGeometry() with

        let normal = 
            let e1 = triangles.[i1].GetPosition() - triangles.[i0].GetPosition()
            let e2 = triangles.[i2].GetPosition() - triangles.[i0].GetPosition()
            Vector4(Vector3.Cross(e1, e2), 0.0f)

        let localToWorld =
            let v0 = triangles.[i0].GetPosition()
            let e1 = triangles.[i1].GetPosition() - v0
            let e2 = triangles.[i2].GetPosition() - v0
            Matrix4x4(e1.X, e1.Y, e1.Z, 0.0f,
                        e2.X, e2.Y, e2.Z, 0.0f,
                        1.0f, 0.0f, 0.0f, 0.0f,
                        v0.X, v0.Y, v0.Z, 1.0f)

        let worldToLocal = 
            let v0 = triangles.[i0].GetPosition()
            let v1 = triangles.[i1].GetPosition()
            let v2 = triangles.[i2].GetPosition()
            let e1 = v1 - v0
            let e2 = v2 - v0

            let crossV2V0 = Vector3.Cross(v2, v0)
            let crossV1V0 = Vector3.Cross(v1, v0)
            let normalXAbs = MathF.Abs(normal.X)
            let normalYAbs = MathF.Abs(normal.Y)
            let normalZAbs = MathF.Abs(normal.Z)
            let normalVec3 = Vector.ToVec3(normal)
            let nDotV1 = Vector.InMemoryDotProduct(ref normalVec3, ref v1)
            if normalXAbs > normalYAbs && normalXAbs > normalYAbs then
                Matrix4x4(0.0f, 0.0f, 1.0f, 0.0f, 
                          e2.Z/normal.X, -e1.Z/normal.X, normal.Y/normal.Z, 0.0f,
                          -e2.Y/normal.X, e1.Y/normal.X, normal.Z/normal.X, 0.0f,
                          crossV2V0.X/normal.X, crossV1V0.X/normal.X, -nDotV1/normal.X, 1.0f)
            elif normalYAbs > normalXAbs && normalYAbs > normalZAbs then
                Matrix4x4(-e2.Z/normal.Y, e1.Z/normal.Y, normal.X/normal.Y, 0.0f,
                          0.0f, 1.0f, 0.0f, 0.0f,
                          e2.X/normal.Y, -e1.X/normal.Y, normal.Z/normal.Y, 0.0f,
                          crossV2V0.Y/normal.Y, -crossV1V0.Y/normal.Y, -nDotV1/normal.Y, 1.0f)
            else Matrix4x4(e2.Y/normal.Z, -e1.Y/normal.Z, normal.X/normal.Z, 0.0f,
                           -e2.X/normal.Z, e1.X/normal.Z, normal.Y/normal.Z, 0.0f,
                           0.0f, 0.0f, 1.0f, 0.0f,
                           crossV2V0.Z/normal.Z, -crossV1V0.Z/normal.Z, -nDotV1/normal.Z, 1.0f)

        interface Hitable with
            override this.TMin = 0.000001f

            override this.NormalForSurfacePoint _ = normal

            override this.IntersectionAcceptable hasIntersection t _ _ =
                hasIntersection && t > this.AsHitable.TMin

            override this.HasIntersection (ray:Ray) = 
                let (hasIntersection,_) = this.AsHitable.Intersect ray 
                hasIntersection

            override this.Intersect (ray:Ray) =

                let rayOriginHomogeneous = ray.Origin
                let rayOriginInTriangleSpace = Vector4.Transform(rayOriginHomogeneous, worldToLocal)
                let rayDirectionInTriangleSpace = Vector4.Transform(ray.Direction, worldToLocal)
                let t = -rayOriginInTriangleSpace.Z/rayDirectionInTriangleSpace.Z
                let barycentric = rayOriginInTriangleSpace + t*rayDirectionInTriangleSpace

                let intersectionWorld = Vector4.Transform(barycentric, localToWorld)
                let tWorld = Vector4.Distance(intersectionWorld, rayOriginHomogeneous)

                (0.0f <= barycentric.X && barycentric.X <= 1.0f && 
                 0.0f <= barycentric.Y && barycentric.Y <= 1.0f &&
                 barycentric.X+barycentric.Y <= 1.0f, tWorld)

        interface AxisAlignedBoundable with
            override this.GetBounds =

                let v0 = triangles.[i0].GetPosition()
                let v1 = triangles.[i1].GetPosition()
                let v2 = triangles.[i2].GetPosition()

                let pMin = Vector4(MathF.Min(v0.X, MathF.Min(v1.X, v2.X)), MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)), MathF.Min(v0.Z, MathF.Min(v1.Z, v2.Z)), 0.0f)

                let pMax = Vector4(MathF.Max(v0.X, MathF.Max(v1.X, v2.X)), MathF.Max(v0.Y, MathF.Max(v1.Y, v2.Y)), MathF.Max(v0.Z, MathF.Max(v1.Z, v2.Z)), 0.0f)

                AABB(pMin, pMax)

            override this.IsBoundable = true
            
        static member CreateTriangleFromVertexStructs<'T when 'T : struct and 'T :> VertexLocateable>(a : 'T, b : 'T, c : 'T)
           = Triangle(a.GetPosition(), b.GetPosition(), c.GetPosition())
             





