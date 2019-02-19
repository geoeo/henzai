namespace HenzaiFunc.Core.RaytraceGeometry

open System
open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Numerics
open Henzai.Core.VertexGeometry

// Baldwin-Weber ray-triangle intersection algorithm: http://jcgt.org/published/0005/03/03/
/// Vertices are CCW
type Triangle(v0 : Point, v1 : Point, v2 : Point) =
    inherit RaytracingGeometry() with

        let v0 = v0

        let v1 = v1

        let v2 = v2

        let e1 = v1 - v0

        let e2 = v2 - v0

        let normal = Vector3.Cross(e1, e2)

        let localToWorld 
            = Matrix4x4(e1.X, e1.Y, e1.Z, 0.0f,
                        e2.X, e2.Y, e2.Z, 0.0f,
                        1.0f, 0.0f, 0.0f, 0.0f,
                        v0.X, v0.Y, v0.Z, 1.0f)

        let worldToLocal = 
            let crossV2V0 = Vector3.Cross(v2, v0)
            let crossV1V0 = Vector3.Cross(v1, v0)
            let normalXAbs = MathF.Abs(normal.X)
            let normalYAbs = MathF.Abs(normal.Y)
            let normalZAbs = MathF.Abs(normal.Z)
            let nDotV1 = Vector3.Dot(normal, v1)
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

                let rayOriginHomogeneous = Vector.ToHomogeneous(ref ray.Origin, 1.0f)
                let rayOriginInTriangleSpace = Vector4.Transform(rayOriginHomogeneous, worldToLocal)
                let rayDirectionInTriangleSpace = Vector4.Transform(Vector.ToHomogeneous(ref ray.Direction, 0.0f), worldToLocal)
                let t = -rayOriginInTriangleSpace.Z/rayDirectionInTriangleSpace.Z
                let barycentric = rayOriginInTriangleSpace + t*rayDirectionInTriangleSpace

                let intersectionWorld = Vector4.Transform(barycentric, localToWorld)
                let tWorld = Vector4.Distance(intersectionWorld, rayOriginHomogeneous)

                (0.0f <= barycentric.X && barycentric.X <= 1.0f && 
                 0.0f <= barycentric.Y && barycentric.Y <= 1.0f &&
                 barycentric.X+barycentric.Y <= 1.0f, tWorld)

        interface AxisAlignedBoundable with
            override this.GetBounds =

                let pMin = Vector3(MathF.Min(v0.X, MathF.Min(v1.X, v2.X)), MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)), MathF.Min(v0.Z, MathF.Min(v1.Z, v2.Z)))

                let pMax = Vector3(MathF.Max(v0.X, MathF.Min(v1.X, v2.X)), MathF.Max(v0.Y, MathF.Min(v1.Y, v2.Y)), MathF.Max(v0.Z, MathF.Min(v1.Z, v2.Z)))

                AABB(pMin, pMax)

            override this.IsBoundable = true


        static member CreateTriangleFromVertexStructs<'T when 'T : struct and 'T :> VertexLocateable>(a : 'T, b : 'T, c : 'T)
           = Triangle(a.GetPosition(), b.GetPosition(), c.GetPosition())

             






