module Raytracer.Geometry.Triangle

open System
open System.Numerics
open Raytracer.Numerics
open Raytracer.Geometry.Core

// Baldwin-Weber ray-triangle intersection algorithm: http://jcgt.org/published/0005/03/03/
type Triangle(v0 : Point, v1 : Point, v2 : Point) =
    inherit Hitable() with

        override this.TMin = 0.000001f

        member this.V0 = v0

        member this.V1 = v1

        member this.V2 = v2

        member this.E1 = v1 - v0

        member this.E2 = v2 - v0

        member this.Normal = Vector3.Cross(this.E1, this.E2)

        member this.LocalToWorld 
            = Matrix4x4(this.E1.X, this.E1.Y, this.E1.Z, 0.0f,
                        this.E2.X, this.E2.Y, this.E2.Z, 0.0f,
                        1.0f, 0.0f, 0.0f, 0.0f,
                        this.V0.X, this.V0.Y, this.V0.Z, 1.0f)

        member this.WorldToLocal = 
            let crossV2V0 = Vector3.Cross(this.V2, this.V0)
            let crossV1V0 = Vector3.Cross(this.V1, this.V0)
            let normalXAbs = MathF.Abs(this.Normal.X)
            let normalYAbs = MathF.Abs(this.Normal.Y)
            let normalZAbs = MathF.Abs(this.Normal.Z)
            let nDotV1 = Vector3.Dot(this.Normal, this.V1)
            if normalXAbs > normalYAbs && normalXAbs > normalYAbs then
                Matrix4x4(0.0f, 0.0f, 1.0f, 0.0f, 
                          this.E2.Z/this.Normal.X, -this.E1.Z/this.Normal.X, this.Normal.Y/this.Normal.Z, 0.0f,
                          -this.E2.Y/this.Normal.X, this.E1.Y/this.Normal.X, this.Normal.Z/this.Normal.X, 0.0f,
                          crossV2V0.X/this.Normal.X, crossV1V0.X/this.Normal.X, -nDotV1/this.Normal.X, 1.0f)
            elif normalYAbs > normalXAbs && normalYAbs > normalZAbs then
                Matrix4x4(-this.E2.Z/this.Normal.Y, this.E1.Z/this.Normal.Y, this.Normal.X/this.Normal.Y, 0.0f,
                          0.0f, 1.0f, 0.0f, 0.0f,
                          this.E2.X/this.Normal.Y, -this.E1.X/this.Normal.Y, this.Normal.Z/this.Normal.Y, 0.0f,
                          crossV2V0.Y/this.Normal.Y, -crossV1V0.Y/this.Normal.Y, -nDotV1/this.Normal.Y, 1.0f)
            else Matrix4x4(this.E2.Y/this.Normal.Z, -this.E1.Y/this.Normal.Z, this.Normal.X/this.Normal.Z, 0.0f,
                           -this.E2.X/this.Normal.Z, this.E1.X/this.Normal.Z, this.Normal.Y/this.Normal.Z, 0.0f,
                           0.0f, 0.0f, 1.0f, 0.0f,
                           crossV2V0.Z/this.Normal.Z, -crossV1V0.Z/this.Normal.Z, -nDotV1/this.Normal.Z, 1.0f)
        
        override this.NormalForSurfacePoint _ = this.Normal

        override this.IntersectionAcceptable hasIntersection t _ _ =
            hasIntersection && t > this.TMin

        override this.HasIntersection (ray:Ray) = 
            let (hasIntersection,_) = this.Intersect ray 
            hasIntersection

        override this.Intersect (ray:Ray) =
    
            let rayOriginHomogeneous = ToHomogeneous &ray.Origin 1.0f
            let rayOriginInTriangleSpace = Vector4.Transform(rayOriginHomogeneous, this.WorldToLocal)
            let rayDirectionInTriangleSpace = Vector4.Transform(ToHomogeneous &ray.Direction 0.0f, this.WorldToLocal)
            let t = -rayOriginInTriangleSpace.Z/rayDirectionInTriangleSpace.Z
            let barycentric = rayOriginInTriangleSpace + t*rayDirectionInTriangleSpace

            let intersectionWorld = Vector4.Transform(barycentric, this.LocalToWorld)
            let tWorld = Vector4.Distance(intersectionWorld, rayOriginHomogeneous)

            (0.0f <= barycentric.X && barycentric.X <= 1.0f && 
             0.0f <= barycentric.Y && barycentric.Y <= 1.0f &&
             barycentric.X+barycentric.Y <= 1.0f, tWorld)



             






